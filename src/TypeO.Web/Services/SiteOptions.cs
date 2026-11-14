namespace TypeO.Web.Services;

public class SiteOptions
{
    public string GitHubOwner { get; set; } = "Udrian";
    public string GitHubOwnerUrl { get; set; } = "https://github.com/Udrian";
    public string ContactEmail { get; set; } = "";
    public List<RepoLink> Repositories { get; set; } = new();
}

public class RepoLink
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string Description { get; set; } = "";
}

public class DownloadOptions
{
    public string ReleasesUrl { get; set; } = "";
    public List<DownloadTarget> Targets { get; set; } = new();
}

public class DownloadTarget
{
    public string Name { get; set; } = "";
    public string Rid { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
}
