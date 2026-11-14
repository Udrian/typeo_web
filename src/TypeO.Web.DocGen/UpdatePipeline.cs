using System.Text.Json;
using System.Text.RegularExpressions;
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
            foreach (var project in typeo.Projects.Where(p => p.Type != "test"))
            {
                var csproj = Path.Combine(cloneDir, project.Name, project.Name + ".csproj");
                Proc.Run("dotnet",
                    $"build \"{csproj}\" -c Release " +
                    "-p:GenerateDocumentationFile=true " +
                    "-p:CopyLocalLockFileAssemblies=true " +
                    "-p:TypeODependenciesResolved=true");
            }

            var documentation = RepoExtractor.Extract(cloneDir, tag, "Release");
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
