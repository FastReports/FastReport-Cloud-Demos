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
        private const string ApiKey = "9999999999999999999999999999999999999999999999999999";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://fastreport.cloud");
            httpClient.DefaultRequestHeaders.Authorization = new FastReportCloudApiKeyHeader(ApiKey);
            var subscriptions = new SubscriptionsClient(httpClient);

            var subscription = (await subscriptions.GetSubscriptionsAsync(0, 10)).Subscriptions.FirstOrDefault();

            TasksClient tasksClient = new TasksClient(httpClient);

            // Create a new task
            await tasksClient.CreateTaskAsync(new CreatePrepareTemplateTaskVM
            {
                Name = "My first task",
                Type = TaskType.Prepare,
                InputFile = new InputFileVM
                {
                    EntityId = "{templateId}"
                },
                OutputFile = new OutputFileVM
                {
                    FileName = "My first task generated file.fpx",
                    FolderId = "{reports folder id}"
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
                            FolderId = "{exports folder id}"
                        }
                    }
                }
            });

            // Get first 100 tasks from the subscription
            var tasks = await tasksClient.GetListAsync(0, 100, subscription.Id);

            // Get last task
            string id = tasks.Tasks.LastOrDefault().Id;

            // Run last task
            await tasksClient.RunTaskByIdAsync(id);

            // Delete all tasks
            foreach (var t in tasks.Tasks)
            {
                await tasksClient.DeleteTaskAsync(t.Id);
            }
        }
    }
}
