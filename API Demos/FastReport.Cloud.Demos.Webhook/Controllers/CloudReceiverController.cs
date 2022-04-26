using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastReport.Cloud.Demos.Webhook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudReceiverController : ControllerBase
    {

        private readonly ILogger<CloudReceiverController> _logger;

        public CloudReceiverController(ILogger<CloudReceiverController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<List<long>> Receiver(IFormFileCollection files)
        {
            List<long> fileLengths = new List<long>();
            foreach (var img in files)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    img.OpenReadStream().CopyTo(ms);
                    fileLengths.Add(ms.ToArray().Length);
                }
            }
            return Ok(fileLengths);
        }
    }
}
