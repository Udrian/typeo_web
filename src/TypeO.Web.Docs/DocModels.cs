using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeO.Web.Docs;

/// <summary>Generated API documentation for a single module version.</summary>
public class ModuleDocumentation
{
    public string Module { get; set; } = "";
    public string Version { get; set; } = "";
    public DateTime GeneratedAtUtc { get; set; }
    public List<ProjectDoc> Projects { get; set; } = new();
}

/// <summary>A single C# project inside a module (e.g. TypeOTK = game, TypeDTK = editor).</summary>
public class ProjectDoc
{
    public string Name { get; set; } = "";

    /// <summary>"game" for runtime projects, "editor" for dev (TypeDitor) projects.</summary>
    public string Kind { get; set; } = "game";

    public List<NamespaceDoc> Namespaces { get; set; } = new();
}

public class NamespaceDoc
{
    public string Name { get; set; } = "";
    public List<TypeDoc> Types { get; set; } = new();
}

public class TypeDoc
{
    /// <summary>XML documentation id without prefix, e.g. "TypeD.Models.ModuleModel".</summary>
    public string Id { get; set; } = "";

    /// <summary>Short display name, e.g. "ModuleModel" or "Grid&lt;T&gt;".</summary>
    public string Name { get; set; } = "";

    public string Namespace { get; set; } = "";

    /// <summary>Class, Struct, Interface, Enum or Delegate.</summary>
    public string Kind { get; set; } = "Class";

    public string Signature { get; set; } = "";
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public List<MemberDoc> Members { get; set; } = new();
}

public class MemberDoc
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>Constructor, Method, Property, Field, Event or EnumValue.</summary>
    public string Kind { get; set; } = "Method";

    public string Signature { get; set; } = "";
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Returns { get; set; }
    public List<ParameterDoc> Parameters { get; set; } = new();
}

public class ParameterDoc
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>The list of modules the website knows about (modules.json).</summary>
public class ModuleRegistry
{
    public List<ModuleInfo> Modules { get; set; } = new();
}

public class ModuleInfo
{
    public string Name { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Repo { get; set; } = "";
    public string Description { get; set; } = "";

    [JsonIgnore]
    public string GitHubUrl => $"https://github.com/{Owner}/{Repo}";

    [JsonIgnore]
    public string GitUrl => $"https://github.com/{Owner}/{Repo}.git";
}

public static class DocsJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };
}
