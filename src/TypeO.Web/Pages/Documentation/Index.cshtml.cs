using Microsoft.AspNetCore.Mvc.RazorPages;
using TypeO.Web.Services;

namespace TypeO.Web.Pages.Documentation;

public class IndexModel : PageModel
{
    public IndexModel(DocumentationService documentation) => Documentation = documentation;

    public DocumentationService Documentation { get; }
}
