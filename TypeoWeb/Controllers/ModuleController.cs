using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
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

        private class Module
        {
            public string name { get; set; }
            public string version { get; set; }
            public List<string> externals { get; set; }
            public List<string> dependencies { get; set; }
            public string type { get; set; }
        }

        [HttpGet("list")]
        public async Task<JsonResult> GetListAsync()
        {
            var result = await s3.ListObjectsV2Async(new ListObjectsV2Request() { BucketName = "typedeaf" });

            var modules = new Dictionary<string, List<Module>>();
            foreach (var obj in result.S3Objects)
            {
                if (!obj.Key.StartsWith("typeo/releases/modules/")) continue;
                if (!obj.Key.EndsWith("product")) continue;
                var name = obj.Key.Remove(0, ("typeo/releases/modules/").Length);
                name = name.Remove(name.IndexOf("/"));
                var version = obj.Key.Remove(0, ($"typeo/releases/modules/{name}/").Length);
                version = version.Remove(version.IndexOf("/"));

                var getResult = await s3.GetObjectAsync(new GetObjectRequest() { BucketName = "typedeaf", Key = obj.Key });
                var json = await new StreamReader(getResult.ResponseStream, Encoding.Latin1).ReadToEndAsync();
                
                var product = (Module)JsonSerializer.Deserialize(json, typeof(Module));
                product.version = version;

                if (!modules.ContainsKey(product.name))
                    modules.Add(product.name, new List<Module>());

                modules[product.name].Add(product);
            }

            return new JsonResult(new { Modules = modules});
        }
    }
}
