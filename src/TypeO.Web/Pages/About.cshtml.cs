using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TypeO.Web.Services;

namespace TypeO.Web.Pages;

public class AboutModel : PageModel
{
    public AboutModel(IOptions<SiteOptions> site) => Site = site.Value;

    public SiteOptions Site { get; }
}
