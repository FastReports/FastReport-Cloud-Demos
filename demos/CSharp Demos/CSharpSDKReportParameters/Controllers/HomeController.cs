using CSharpSDKReportParameters.Models;
using FastReport.Cloud;
using FastReport.Cloud.ReportProcessor;
using FastReport.Cloud.ResultsProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSDKReportParameters.Controllers
{
    public class HomeController : Controller
    {
        private const string BarcodeReport = "77u/PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxSZXBvcnQgU2NyaXB0TGFuZ3VhZ2U9IkNTaGFycCIgUmVwb3J0SW5mby5DcmVhdGVkPSIxMi8wNy8yMDIwIDE1OjI0OjUwIiBSZXBvcnRJbmZvLk1vZGlmaWVkPSIxMi8wNy8yMDIwIDE1OjQ0OjQ4IiBSZXBvcnRJbmZvLkNyZWF0b3JWZXJzaW9uPSIxLjAuMC4wIj4NCiAgPFNjcmlwdFRleHQ+dXNpbmcgU3lzdGVtOw0KdXNpbmcgU3lzdGVtLkNvbGxlY3Rpb25zOw0KdXNpbmcgU3lzdGVtLkNvbGxlY3Rpb25zLkdlbmVyaWM7DQp1c2luZyBTeXN0ZW0uQ29tcG9uZW50TW9kZWw7DQp1c2luZyBTeXN0ZW0uV2luZG93cy5Gb3JtczsNCnVzaW5nIFN5c3RlbS5EcmF3aW5nOw0KdXNpbmcgU3lzdGVtLkRhdGE7DQp1c2luZyBGYXN0UmVwb3J0Ow0KdXNpbmcgRmFzdFJlcG9ydC5EYXRhOw0KdXNpbmcgRmFzdFJlcG9ydC5EaWFsb2c7DQp1c2luZyBGYXN0UmVwb3J0LkJhcmNvZGU7DQp1c2luZyBGYXN0UmVwb3J0LlRhYmxlOw0KdXNpbmcgRmFzdFJlcG9ydC5VdGlsczsNCg0KbmFtZXNwYWNlIEZhc3RSZXBvcnQNCnsNCiAgcHVibGljIGNsYXNzIFJlcG9ydFNjcmlwdA0KICB7DQogICANCiAgfQ0KfQ0KPC9TY3JpcHRUZXh0Pg0KICA8RGljdGlvbmFyeT4NCiAgICA8UGFyYW1ldGVyIE5hbWU9InJlcG9ydF9uYW1lIiBEYXRhVHlwZT0iU3lzdGVtLlN0cmluZyIgRXhwcmVzc2lvbj0iJnF1b3Q7Q3ViZSBQb3NpdGlvbiZxdW90OyIvPg0KICAgIDxQYXJhbWV0ZXIgTmFtZT0iYmFyY29kZV9udW1zIiBEYXRhVHlwZT0iU3lzdGVtLlN0cmluZyIgRXhwcmVzc2lvbj0iJnF1b3Q7MjE0MzIyMjQmcXVvdDsiLz4NCiAgPC9EaWN0aW9uYXJ5Pg0KICA8UmVwb3J0UGFnZSBOYW1lPSJQYWdlMSIgV2F0ZXJtYXJrLkZvbnQ9IkFyaWFsLCA2MHB0Ij4NCiAgICA8UmVwb3J0VGl0bGVCYW5kIE5hbWU9IlJlcG9ydFRpdGxlMSIgV2lkdGg9IjcxOC4yIiBIZWlnaHQ9IjM3LjgiPg0KICAgICAgPFRleHRPYmplY3QgTmFtZT0iVGV4dDEiIExlZnQ9IjkuNDUiIFRvcD0iOS40NSIgV2lkdGg9IjIwNy45IiBIZWlnaHQ9IjE4LjkiIFRleHQ9IltyZXBvcnRfbmFtZV0iIEZvbnQ9IkFyaWFsLCAxMHB0Ii8+DQogICAgPC9SZXBvcnRUaXRsZUJhbmQ+DQogICAgPERhdGFCYW5kIE5hbWU9IkRhdGExIiBUb3A9IjQxLjgiIFdpZHRoPSI3MTguMiIgSGVpZ2h0PSIxMDMuOTUiPg0KICAgICAgPEJhcmNvZGVPYmplY3QgTmFtZT0iQmFyY29kZTEiIExlZnQ9IjE4LjkiIFRvcD0iOS40NSIgV2lkdGg9IjExNSIgSGVpZ2h0PSI4NS4wNSIgVGV4dD0iW2JhcmNvZGVfbnVtc10iIEFsbG93RXhwcmVzc2lvbnM9InRydWUiIEJhcmNvZGU9IjIvNSBJbnRlcmxlYXZlZCIvPg0KICAgIDwvRGF0YUJhbmQ+DQogIDwvUmVwb3J0UGFnZT4NCjwvUmVwb3J0Pg0K";
        private readonly ILogger<HomeController> logger;
        private readonly ISubscriptionsClient subscriptionsClient;
        private readonly ITemplatesClient templatesClient;
        private readonly IExportsClient exportsClient;
        private readonly IDownloadClient downloadClient;

        public static TemplateVM ReportTemplate { get; set; }

        public HomeController(
            ILogger<HomeController> logger,
            ISubscriptionsClient subscriptionsClient,
            ITemplatesClient templatesClient,
            IExportsClient exportsClient,
            IDownloadClient downloadClient)
        {
            this.logger = logger;
            this.subscriptionsClient = subscriptionsClient;
            this.templatesClient = templatesClient;
            this.exportsClient = exportsClient;
            this.downloadClient = downloadClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> PrepareReport(IFormCollection forms)
        {
            var subscription = (await subscriptionsClient.GetSubscriptionsAsync(0, 1)).Subscriptions.First();
            var template = await SendTemplateToCloud(subscription.TemplatesFolder.FolderId);

            var name = forms["ReportName"][0];
            var nums = forms["BarcodeNums"][0];

            Dictionary<string, object> parameters = new();
            parameters.Add("report_name", name);
            parameters.Add("barcode_nums", nums);

            
            
            ExportTemplateTaskVM eTaskVM = new()
            {
                FileName = "songs.pdf",
                Format = ExportTemplateTaskVMFormat.Pdf,
                ReportParameters = parameters,
                FolderId = subscription.ExportsFolder.FolderId
            };
            var exportedFile = await templatesClient.ExportAsync(template.Id, eTaskVM);

            int attempts = 5;

            string fileId = exportedFile.Id;

            while (exportedFile.Status != ExportVMStatus.Success && attempts >= 0)
            {
                await Task.Delay(1000);
                exportedFile = exportsClient.GetFile(fileId);
                attempts--;
            }


            var fileStream = await downloadClient.GetExportAsync(fileId);
            using (MemoryStream ms = new MemoryStream())
            {
                fileStream.Stream.CopyTo(ms);
                return new FileContentResult(ms.ToArray(), "application/octet-stream")
                {
                    FileDownloadName = "report.pdf"
                };
            }
        }

        private async Task<TemplateVM> SendTemplateToCloud(string folderId)
        {
            if (ReportTemplate == null)
            {

                TemplateCreateVM tCreateVM = new()
                {
                    Name = "pricelist.frx",
                    Content = BarcodeReport
                };

                ReportTemplate = await templatesClient.UploadFileAsync(folderId, tCreateVM);
            }
            return ReportTemplate;
        }

       
    }
}
