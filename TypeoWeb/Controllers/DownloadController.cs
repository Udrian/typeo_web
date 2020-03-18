using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TypeoWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DownloadController : ControllerBase
    {
        private ILogger<DownloadController> Logger { get; set; }

        public DownloadController(ILogger<DownloadController> logger)
        {
            Logger = logger;
        }

        [HttpGet("{file}")]
        public IActionResult Get([FromRoute] string file, [FromQuery] string path)
        {
            path = (string.IsNullOrEmpty(path) ? "" : path + "/");
            return Redirect($"https://typedeaf.nyc3.cdn.digitaloceanspaces.com/typeo/releases/{path}{file}");
        }
    }
}
