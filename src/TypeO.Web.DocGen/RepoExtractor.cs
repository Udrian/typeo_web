using System.Text.Json;
using TypeO.Web.Docs;

namespace TypeO.Web.DocGen;

/// <summary>Extracts documentation from an already generated and built module repository.</summary>
public static class RepoExtractor
{
    public static ModuleDocumentation Extract(string repoPath, string? versionOverride, string configuration)
    {
        var typeo = TypeoJson.Load(repoPath);
        var projects = typeo.Projects.Where(p => p.Type != "test").ToList();

        var binDirs = projects
            .Select(p => Path.Combine(repoPath, p.Name, "bin", configuration, "net10.0"))
            .Where(Directory.Exists)
            .ToList();

        var documentation = new ModuleDocumentation
        {
            Module = typeo.Name,
            Version = versionOverride ?? typeo.Version,
            GeneratedAtUtc = DateTime.UtcNow,
        };

        foreach (var project in projects)
        {
            var binDir = Path.Combine(repoPath, project.Name, "bin", configuration, "net10.0");
            var assemblyPath = Path.Combine(binDir, project.Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                Console.Error.WriteLine($"warning: skipping project {project.Name}: built assembly not found at {assemblyPath}");
                continue;
            }

            var xmlPath = Path.Combine(binDir, project.Name + ".xml");
            var kind = project.Type == "dev" ? "editor" : "game";

            Console.WriteLine($"Extracting {project.Name} ({kind}) from {assemblyPath}");
            documentation.Projects.Add(ApiExtractor.Extract(
                assemblyPath,
                File.Exists(xmlPath) ? xmlPath : null,
                kind,
                binDirs.Where(d => d != binDir)));
        }

        if (documentation.Projects.Count == 0)
            throw new InvalidOperationException(
                $"No built assemblies found for {typeo.Name}. Build the module first.");

        return documentation;
    }

    public static string Write(ModuleDocumentation documentation, string outputDir)
    {
        var moduleDir = Path.Combine(outputDir, documentation.Module);
        Directory.CreateDirectory(moduleDir);
        var path = Path.Combine(moduleDir, documentation.Version + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(documentation, DocsJson.Options));
        Console.WriteLine($"Wrote {path}");
        return path;
    }
}
