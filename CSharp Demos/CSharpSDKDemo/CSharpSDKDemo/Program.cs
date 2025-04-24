using FastReport.Cloud;
using FastReport.Cloud.Client;
class Program
{
    private const string ApiKey = "**YOUR APIKEY HERE**";

    static async Task Main()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://fastreport.cloud");
        httpClient.DefaultRequestHeaders.Authorization = new FastReportCloudApiKeyHeader(ApiKey);
        var subscriptionsClient = new SubscriptionsClient(httpClient);

        var templatesClient = new TemplatesClient(httpClient);
        var exportsClient = new ExportsClient(httpClient);
        var downloadClient = new DownloadClient(httpClient);

        var subscription = (await subscriptionsClient.GetSubscriptionsAsync(0, 10)).Subscriptions.First();

        var templateRootFolder = subscription.TemplatesFolder.FolderId;
        var exportRootFolder = subscription.ExportsFolder.FolderId;

        // uploading template
        TemplateVM uploadedTemplate;
        using (FileStream fileStream = new FileStream("../../../../Box.frx", FileMode.Open, FileAccess.Read))
        {
            FileParameter fileContent = new FileParameter(fileStream);
            uploadedTemplate = await templatesClient.UploadFileV2Async(templateRootFolder, null, null, fileContent);
        }
        Console.WriteLine("Successful template upload!");

        // exporting template 
        ExportTemplateVM export = new ExportTemplateVM()
        {
            FileName = "Box",
            FolderId = exportRootFolder,
            Format = ExportFormat.Pdf
        };

        ExportVM exportedFile = new ExportVM();
        try
        {
            exportedFile = await templatesClient.ExportAsync(uploadedTemplate.Id, export);
            Console.WriteLine("Successful template export!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        int attempts = 3;
        while (exportedFile.Status != FileStatus.Success && attempts >= 0)
        {
            await Task.Delay(1000);
            exportedFile = exportsClient.GetFile(exportedFile.Id);
            attempts--;
        }

        // downloading file
        using (var file = await downloadClient.GetExportAsync(exportedFile.Id))
        {
            using (var pdf = File.Open("../../../../box.pdf", FileMode.Create))
            {
                file.Stream.CopyTo(pdf);
            }
        }
        Console.WriteLine("Successful pdf download!");

    }
}