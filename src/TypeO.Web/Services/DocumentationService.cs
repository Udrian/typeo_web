using System.Collections.Concurrent;
using System.Text.Json;
using TypeO.Web.Docs;

namespace TypeO.Web.Services;

/// <summary>
/// Serves the module registry (modules.json) and the generated API documentation
/// (wwwroot/docs-data/{Module}/{version}.json) produced by TypeO.Web.DocGen.
/// </summary>
public class DocumentationService
{
    private readonly string docsDataPath;
    private readonly ConcurrentDictionary<string, ModuleDocumentation?> cache = new(StringComparer.OrdinalIgnoreCase);

    public DocumentationService(IWebHostEnvironment environment)
    {
        docsDataPath = Path.Combine(environment.WebRootPath ?? "", "docs-data");

        var registryPath = Path.Combine(environment.ContentRootPath, "modules.json");
        var registry = File.Exists(registryPath)
            ? JsonSerializer.Deserialize<ModuleRegistry>(File.ReadAllText(registryPath), DocsJson.Options)
            : null;
        Modules = registry?.Modules ?? new List<ModuleInfo>();
    }

    public IReadOnlyList<ModuleInfo> Modules { get; }

    public ModuleInfo? FindModule(string name) =>
        Modules.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Available documentation versions for a module, newest first.</summary>
    public List<string> GetVersions(string module)
    {
        var dir = Path.Combine(docsDataPath, module);
        if (!Directory.Exists(dir))
            return new List<string>();

        return Directory.EnumerateFiles(dir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(v => v != null)
            .Select(v => v!)
            .OrderByDescending(v => Version.TryParse(v, out var parsed) ? parsed : new Version(0, 0))
            .ThenByDescending(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public string? GetLatestVersion(string module) => GetVersions(module).FirstOrDefault();

    public ModuleDocumentation? Load(string module, string version)
    {
        // Validate against the directory listing to avoid path traversal via route values.
        if (!GetVersions(module).Contains(version, StringComparer.OrdinalIgnoreCase))
            return null;

        return cache.GetOrAdd($"{module}/{version}", _ =>
        {
            var path = Path.Combine(docsDataPath, module, version + ".json");
            if (!File.Exists(path))
                return null;
            return JsonSerializer.Deserialize<ModuleDocumentation>(File.ReadAllText(path), DocsJson.Options);
        });
    }
}
