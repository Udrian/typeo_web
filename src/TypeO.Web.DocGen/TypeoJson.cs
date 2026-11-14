using System.Text.Json;

namespace TypeO.Web.DocGen;

/// <summary>Minimal reader for a module repo's typeo.json.</summary>
public class TypeoJson
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public List<TypeoProject> Projects { get; set; } = new();

    public static TypeoJson Load(string repoPath)
    {
        var path = Path.Combine(repoPath, "typeo.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;

        var result = new TypeoJson
        {
            Name = root.GetProperty("name").GetString() ?? "",
            Version = root.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "",
        };

        if (root.TryGetProperty("project", out var projects))
        {
            foreach (var project in projects.EnumerateArray())
            {
                result.Projects.Add(new TypeoProject
                {
                    Name = project.GetProperty("Name").GetString() ?? "",
                    Type = project.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "",
                });
            }
        }

        return result;
    }
}

public class TypeoProject
{
    public string Name { get; set; } = "";

    /// <summary>"" = game/runtime project, "dev" = editor project, "test" = test project.</summary>
    public string Type { get; set; } = "";
}
