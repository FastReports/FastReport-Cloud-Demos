using FastReport.Cloud;
using FastReport.Cloud.Management;
using FastReport.Cloud.ReportProcessor;
using FastReport.Cloud.ResultsProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSDKDemo
{
    class Program
    {
        private const string ApiKey = "PUT YOUR APIKEY HERE";



        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://fastreport.cloud");
            httpClient.DefaultRequestHeaders.Authorization = new FastReportCloudApiKeyHeader(ApiKey);
            var subscriptions = new SubscriptionsClient(httpClient);

            var rpClientTemplates = new TemplatesClient(httpClient);
            var rpClientExports = new ExportsClient(httpClient);
            var downloadClient = new DownloadClient(httpClient);

            var subscription = (await subscriptions.GetSubscriptionsAsync(0, 10)).Subscriptions.First();

            var templateFolder = subscription.TemplatesFolder.FolderId;
            var exportFolder = subscription.ExportsFolder.FolderId;
            Console.WriteLine("Creating template");
            TemplateCreateVM templateCreateVM = new TemplateCreateVM()
            {
                Name = "box.frx",
                Content = TestData.BoxReport
            };

            Console.WriteLine("Uploadind template");
            TemplateVM uploadedFile = await rpClientTemplates.UploadFileAsync(templateFolder, templateCreateVM);

            Console.WriteLine("Creating pdf");
            ExportTemplateTaskVM export = new ExportTemplateTaskVM()
            {
                FileName = "box.pdf",
                FolderId = exportFolder,
                Format = ExportTemplateTaskVMFormat.Pdf
            };
            Console.WriteLine("Exporting pdf");
            ExportVM exportedFile = await rpClientTemplates.ExportAsync(uploadedFile.Id, export) as ExportVM;
            string fileId = exportedFile.Id;
            int attempts = 3;

            exportedFile = rpClientExports.GetFile(fileId);
            while (exportedFile.Status != ExportVMStatus.Success && attempts >= 0)
            {
                await Task.Delay(1000);
                exportedFile = rpClientExports.GetFile(fileId);
                attempts--;
            }


            Console.WriteLine("Downloading pdf");
            using (var file = await downloadClient.GetExportAsync(fileId))
            {
                using (var pdf = File.Open("report.pdf", FileMode.Create))
                {
                    file.Stream.CopyTo(pdf);
                }
            }
            Console.WriteLine("Success!");
        }
    }
}
