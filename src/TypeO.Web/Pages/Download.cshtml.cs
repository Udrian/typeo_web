using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TypeO.Web.Services;

namespace TypeO.Web.Pages;

public class DownloadModel : PageModel
{
    public DownloadModel(IOptions<DownloadOptions> downloads) => Downloads = downloads.Value;

    public DownloadOptions Downloads { get; }
}
