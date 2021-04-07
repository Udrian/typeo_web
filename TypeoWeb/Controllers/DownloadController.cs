using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public IActionResult GetFile([FromRoute] string file)
        {
            return Redirect($"https://typedeaf.nyc3.cdn.digitaloceanspaces.com/typeo/releases/{file}");
        }

        [HttpGet("{path}/{file}")]
        public IActionResult GetFilePath([FromRoute] string path, [FromRoute] string file)
        {
            path = (string.IsNullOrEmpty(path) ? "" : path + "/");
            return Redirect($"https://typedeaf.nyc3.cdn.digitaloceanspaces.com/typeo/releases/{path}{file}");
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestAsync([FromQuery] string query)
        {
            var key = "ZCBOSFIPCUV5BJBFK7R6";
            var secret = "/kOyUO3uBI6b8oYpJSSGrEMM7/+LWZft23WKqPv7sh4";
            var config = new AmazonS3Config() { ServiceURL = "https://nyc3.digitaloceanspaces.com" };
            var s3 = new AmazonS3Client(key, secret, config);
            var result = await s3.ListObjectsV2Async(new ListObjectsV2Request() { BucketName = "typedeaf" });

            DateTime latest = DateTime.MinValue;
            var returnKey = "";
            foreach(var obj in result.S3Objects)
            {
                if(!obj.Key.StartsWith("typeo/releases/")) continue;
                var objkey = obj.Key.Remove(0, ("typeo/releases/").Length);
                if(Regex.IsMatch(objkey, query) && obj.LastModified > latest)
                {
                    returnKey = objkey;
                    latest = obj.LastModified;
                }
            }

            return Ok(returnKey);
        }

        [HttpGet("releasenotes")]
        public async Task<IActionResult> GetreleasenotesAsync()
        {
            var path = "https://typedeaf.nyc3.cdn.digitaloceanspaces.com/typeo/releases/ReleaseNotes.txt";

            var net = new WebClient();
            var data = net.DownloadData(path);
            var content = new MemoryStream(data);
            var stringContent = Encoding.ASCII.GetString(content.ToArray());

            return Ok(stringContent);
        }
    }
}
