using FastReport.Cloud;
using FastReport.Cloud.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CSharpSDKTasks
{
    class Program
    {
        private const string ApiKey = "PUT YOUR API KEY HERE";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://fastreport.cloud");
            httpClient.DefaultRequestHeaders.Authorization = new FastReportCloudApiKeyHeader(ApiKey);
            var subscriptions = new SubscriptionsClient(httpClient);

            var subscription = (await subscriptions.GetSubscriptionsAsync(0, 10)).Subscriptions.FirstOrDefault();

            TasksClient tasksClient = new TasksClient(httpClient);

            var templateClient = new TemplatesClient(httpClient);
            var templateFolderClient = new TemplateFoldersClient(httpClient);
            var a = new TemplateCreateVM();
            var template = await templateClient.UploadFileAsync((await templateFolderClient.GetRootFolderAsync()).Id, a);

            var reportFolderClient = new ReportFoldersClient(httpClient);

            var exportFolderClient = new ExportFoldersClient(httpClient);

            // Create a new task
            var currentTask = await tasksClient.CreateTaskAsync(new CreatePrepareTemplateTaskVM
            {

                Name = "My first task",
                Type = TaskType.Prepare,
                InputFile = new InputFileVM
                {
                    EntityId = template.Id
                },
                OutputFile = new OutputFileVM
                {
                    FileName = "My first task generated file.fpx",
                    FolderId = (await reportFolderClient.GetRootFolderAsync()).Id,
                },
                Exports = new List<CreateExportReportTaskVM>
                    {
                        new CreateExportReportTaskVM
                        {
                            Type = TaskType.ExportReport,
                            Format = ExportFormat.Pdf,
                            OutputFile = new OutputFileVM
                            {
                                FileName = "pdfFromFpxFromFrx.pdf",
                                FolderId = (await exportFolderClient.GetRootFolderAsync()).Id
                            }
                        }
                    }
            });

            // Run last task
            await tasksClient.RunTaskByIdAsync(currentTask.Id);
            await tasksClient.DeleteTaskAsync(currentTask.Id);
        }
    }
}
