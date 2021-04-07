using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeoWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModuleController : ControllerBase
    {
        private AmazonS3Client s3 { get; set; }
        private ILogger<ModuleController> Logger { get; set; }

        public ModuleController(ILogger<ModuleController> logger)
        {
            Logger = logger;

            var key = "ZCBOSFIPCUV5BJBFK7R6";
            var secret = "/kOyUO3uBI6b8oYpJSSGrEMM7/+LWZft23WKqPv7sh4";
            var config = new AmazonS3Config() { ServiceURL = "https://nyc3.digitaloceanspaces.com" };
            s3 = new AmazonS3Client(key, secret, config);
        }

        [HttpGet("list")]
        public async Task<JsonResult> GetListAsync()
        {
            var result = await s3.ListObjectsV2Async(new ListObjectsV2Request() { BucketName = "typedeaf" });

            var modules = new Dictionary<string, List<string>>();
            foreach (var obj in result.S3Objects)
            {
                if (!obj.Key.StartsWith("typeo/releases/modules/")) continue;
                var name = obj.Key.Remove(0, ("typeo/releases/modules/").Length);
                name = name.Remove(name.IndexOf("/"));
                var version = obj.Key.Remove(0, obj.Key.LastIndexOf("-") + 1);
                version = version.Remove(version.LastIndexOf(".zip"));
                if (!modules.ContainsKey(name))
                    modules.Add(name, new List<string>());
                modules[name].Add(version);
            }

            return new JsonResult(new { Modules = modules});
        }
    }
}
