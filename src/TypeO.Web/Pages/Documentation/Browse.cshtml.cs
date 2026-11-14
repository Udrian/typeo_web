using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TypeO.Web.Docs;
using TypeO.Web.Services;

namespace TypeO.Web.Pages.Documentation;

public class BrowseModel : PageModel
{
    private readonly DocumentationService documentation;

    public BrowseModel(DocumentationService documentation) => this.documentation = documentation;

    public ModuleInfo Module { get; private set; } = null!;
    public string? Version { get; private set; }
    public List<string> Versions { get; private set; } = new();
    public ModuleDocumentation? Documentation { get; private set; }
    public TypeDoc? SelectedType { get; private set; }
    public bool TypeNotFound { get; private set; }

    public IActionResult OnGet(string moduleName, string? version, string? typeId)
    {
        var module = documentation.FindModule(moduleName);
        if (module == null)
            return NotFound();

        Module = module;
        Versions = documentation.GetVersions(module.Name);

        if (Versions.Count == 0)
            return Page();

        if (string.IsNullOrEmpty(version))
            return RedirectToPage(new { moduleName = module.Name, version = Versions[0] });

        Documentation = documentation.Load(module.Name, version);
        if (Documentation == null)
            return NotFound();

        Version = Documentation.Version;

        if (!string.IsNullOrEmpty(typeId))
        {
            SelectedType = Documentation.Projects
                .SelectMany(p => p.Namespaces)
                .SelectMany(n => n.Types)
                .FirstOrDefault(t => string.Equals(t.Id, typeId, StringComparison.Ordinal));
            TypeNotFound = SelectedType == null;
        }

        return Page();
    }

    public string TypeUrl(TypeDoc type, string? version = null) =>
        Url.Page("/Documentation/Browse", new { moduleName = Module.Name, version = version ?? Version, typeId = type.Id })!;

    public string VersionUrl(string version) =>
        Url.Page("/Documentation/Browse", new
        {
            moduleName = Module.Name,
            version,
            typeId = SelectedType?.Id,
        })!;

    public static IEnumerable<(string Heading, List<MemberDoc> Members)> MemberGroups(TypeDoc type)
    {
        var order = new[]
        {
            ("EnumValue", "Values"),
            ("Constructor", "Constructors"),
            ("Property", "Properties"),
            ("Method", "Methods"),
            ("Operator", "Operators"),
            ("Event", "Events"),
            ("Field", "Fields"),
        };

        foreach (var (kind, heading) in order)
        {
            var members = type.Members.Where(m => m.Kind == kind).ToList();
            if (members.Count > 0)
                yield return (heading, members);
        }
    }
}
