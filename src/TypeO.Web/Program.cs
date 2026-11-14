using TypeO.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<DownloadOptions>(builder.Configuration.GetSection("Downloads"));
builder.Services.AddSingleton<DocumentationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
