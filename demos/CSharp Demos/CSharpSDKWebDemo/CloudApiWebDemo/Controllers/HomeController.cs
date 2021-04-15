using CloudApiWebDemo.Models;
using FastReport.Cloud;
using FastReport.Cloud.ReportProcessor;
using FastReport.Cloud.ResultsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudApiWebDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ISubscriptionsClient subscriptionsClient;
        private readonly ITemplatesClient templatesClient;
        private readonly IExportsClient exportsClient;
        private readonly IDownloadClient downloadClient;

        public static TemplateVM ReportTemplate { get; set; }



        //application will upload template from resources folder and create export files on startup
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

        public IActionResult SongView(int id)
        {
            Song entity;
            using (ApplicationContext db = new())
            {
                entity = db.Songs.Where(e => e.Id == id).FirstOrDefault();
            }
            return View(entity);
        }

        public async Task<FileContentResult> GetPDFRecommendations()
        {
            var subscription = (await subscriptionsClient.GetSubscriptionsAsync(0, 1)).Subscriptions.First();

            var template = await SendTemplateToCloud(subscription.TemplatesFolder.FolderId);
            
            ExportTemplateTaskVM eTaskVM = new()
            {
                FileName = "songs.pdf",
                Format = ExportTemplateTaskVMFormat.Pdf,
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
            using(MemoryStream ms = new MemoryStream())
            {
                fileStream.Stream.CopyTo(ms);
                return new FileContentResult(ms.ToArray(), "application/octet-stream")
                {
                    FileDownloadName = "report.pdf"
                };
            }
            
        }

        public async Task<FileContentResult> GetXlsxRecommendations()
        {
            var subscription = (await subscriptionsClient.GetSubscriptionsAsync(0, 1)).Subscriptions.First();

            var template = await SendTemplateToCloud(subscription.TemplatesFolder.FolderId);

            ExportTemplateTaskVM eTaskVM = new()
            {
                FileName = "songs.xlsx",
                Format = ExportTemplateTaskVMFormat.Xlsx,
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
                    FileDownloadName = "report.xlsx"
                };
            }
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

        #region Private Methods
        private async Task<TemplateVM> SendTemplateToCloud(string folderId)
        {
            if (ReportTemplate == null)
            {

                TemplateCreateVM tCreateVM = new()
                {
                    Name = "pricelist.frx",
                    Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + @"/Resources/PriceList.frx"))
                };

                ReportTemplate = await templatesClient.UploadFileAsync(folderId, tCreateVM);
            }
            return ReportTemplate;
        }

        #endregion
    }
}
