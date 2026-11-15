using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using TypeO.Web.Docs;

namespace TypeO.Web.DocGen;

/// <summary>
/// Checks every module in modules.json for release tags that have no generated documentation yet,
/// then clones, generates project files (typer), builds and extracts documentation for each of them.
/// </summary>
public class UpdatePipeline
{
    private static readonly Regex ReleaseTag = new(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

    public required string ModulesJsonPath { get; init; }
    public required string OutputDir { get; init; }
    public required string WorkDir { get; init; }
    public string? ModuleFilter { get; init; }
    public string PythonExecutable { get; init; } = "python";
    public bool KeepWork { get; init; }

    public int Run()
    {
        var registry = JsonSerializer.Deserialize<ModuleRegistry>(File.ReadAllText(ModulesJsonPath), DocsJson.Options)
                       ?? throw new InvalidOperationException($"Could not read {ModulesJsonPath}");

        var failures = 0;
        var generated = 0;

        foreach (var module in registry.Modules)
        {
            if (ModuleFilter != null &&
                !string.Equals(module.Name, ModuleFilter, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(module.Repo, ModuleFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            Console.WriteLine($"=== {module.Name} ({module.GitHubUrl}) ===");

            List<string> tags;
            try
            {
                tags = ListReleaseTags(module);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"error: could not list tags for {module.Name}: {ex.Message}");
                failures++;
                continue;
            }

            foreach (var tag in tags)
            {
                var target = Path.Combine(OutputDir, module.Name, tag + ".json");
                if (File.Exists(target))
                {
                    Console.WriteLine($"{module.Name} {tag}: up to date");
                    continue;
                }

                try
                {
                    Generate(module, tag);
                    generated++;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"error: failed to generate docs for {module.Name} {tag}: {ex.Message}");
                    failures++;
                }
            }
        }

        Console.WriteLine($"Done. Generated {generated} documentation file(s), {failures} failure(s).");
        return failures == 0 ? 0 : 1;
    }

    private List<string> ListReleaseTags(ModuleInfo module)
    {
        var output = Proc.Run("git", $"ls-remote --tags --refs {module.GitUrl}", echo: false);
        var tags = new List<string>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = line.IndexOf("refs/tags/", StringComparison.Ordinal);
            if (index < 0)
                continue;
            var tag = line[(index + "refs/tags/".Length)..];
            if (ReleaseTag.IsMatch(tag))
                tags.Add(tag);
        }
        tags.Sort((a, b) => Version.Parse(a).CompareTo(Version.Parse(b)));
        return tags;
    }

    private void Generate(ModuleInfo module, string tag)
    {
        Console.WriteLine($"--- Generating documentation for {module.Name} {tag} ---");

        var cloneDir = Path.Combine(Path.GetFullPath(WorkDir), $"{module.Name}-{tag}");
        DeleteDirectory(cloneDir);
        Directory.CreateDirectory(Path.GetDirectoryName(cloneDir)!);

        try
        {
            Proc.Run("git", $"clone --quiet --depth 1 --recurse-submodules --shallow-submodules --branch {tag} {module.GitUrl} \"{cloneDir}\"");

            // Generate csproj/solution files; module repos only carry typeo.json.
            // This also resolves module dependencies into the TypeO modules cache.
            Proc.Run(PythonExecutable, $"\"{Path.Combine(cloneDir, "typer", "typer.py")}\" generate -p \"{cloneDir}\"", cloneDir);

            var typeo = TypeoJson.Load(cloneDir);
            AlignPinnedPackageVersions(cloneDir, typeo);

            var builtProjects = new List<string>();
            foreach (var project in typeo.Projects.Where(p => p.Type != "test"))
            {
                var csproj = Path.Combine(cloneDir, project.Name, project.Name + ".csproj");
                try
                {
                    Proc.Run("dotnet",
                        $"build \"{csproj}\" -c Debug " +
                        "-p:GenerateDocumentationFile=true " +
                        "-p:CopyLocalLockFileAssemblies=true " +
                        "-p:TypeODependenciesResolved=true " +
                        // Older module tags pin package versions that transitively downgrade
                        // (e.g. Avalonia via Dock.Model.Avalonia). They built that way historically,
                        // so don't let NU1605 fail documentation builds.
                        "-p:NoWarn=NU1605");
                    builtProjects.Add(project.Name);
                }
                catch (Exception ex)
                {
                    // Document the projects that do build instead of dropping the whole version.
                    Console.Error.WriteLine($"warning: {module.Name} {tag}: project {project.Name} failed to build " +
                                            $"and will be missing from the documentation: {ex.Message}");
                }
            }

            if (builtProjects.Count == 0)
                throw new InvalidOperationException("No project of the module could be built.");

            var documentation = RepoExtractor.Extract(cloneDir, tag, "Debug");
            RepoExtractor.Write(documentation, OutputDir);
        }
        finally
        {
            if (!KeepWork)
            {
                try
                {
                    DeleteDirectory(cloneDir);
                }
                catch (Exception ex)
                {
                    // Never fail a generated version over work dir cleanup.
                    Console.Error.WriteLine($"warning: could not clean up {cloneDir}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// typer generate creates dev projects from the installed "avalonia.app" dotnet template, which
    /// injects package references at the template's (current) Avalonia version. For old tags that
    /// version can be newer than the versions pinned in the module's typeo.json, breaking restore and
    /// the Avalonia XAML compiler. Rewrite any generated PackageReference back to the pinned version.
    /// </summary>
    private static void AlignPinnedPackageVersions(string cloneDir, TypeoJson typeo)
    {
        var pins = typeo.NugetDependencyVersions();
        if (pins.Count == 0)
            return;

        foreach (var project in typeo.Projects.Where(p => p.Type != "test"))
        {
            var csprojPath = Path.Combine(cloneDir, project.Name, project.Name + ".csproj");
            if (!File.Exists(csprojPath))
                continue;

            var csproj = new XmlDocument();
            csproj.Load(csprojPath);

            var changed = false;
            foreach (var package in csproj.GetElementsByTagName("PackageReference").Cast<XmlElement>())
            {
                var name = package.GetAttribute("Include");
                var version = package.GetAttribute("Version");
                if (name.Length > 0 && version.Length > 0 &&
                    pins.TryGetValue(name, out var pinned) && version != pinned)
                {
                    Console.WriteLine($"{project.Name}: aligning {name} {version} -> {pinned} (typeo.json pin)");
                    package.SetAttribute("Version", pinned);
                    changed = true;
                }
            }

            if (changed)
                csproj.Save(csprojPath);
        }
    }

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
            return;

        // Clear read-only attributes that git sets on object files.
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(path, true);
    }
}
