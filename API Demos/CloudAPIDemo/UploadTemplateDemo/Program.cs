using UploadTemplateDemo.Properties;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UploadTemplateDemo
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiKey = "*YOUR API KEY HERE*";

        //View model of the file we want to upload
        private static FileVM boxFileVM = new FileVM()
        {
            name = "createdByScript.frx",
            tags = new string[] { "generated" },
            icon = null,
            // adding file to project resources is unnecessary, you can get it's content with any method you like
            content = Convert.ToBase64String(Resources.Box)
        };

        static async Task Main()
        {
            // you can lookup API documentation here - https://fastreport.cloud/docs/

            CloudRequestSender requestSender = new CloudRequestSender(apiKey);

            dynamic templateRootFolder = await requestSender.SendRequestAsync("https://fastreport.cloud/api/rp/v1/Templates/Root", CloudRequestSender.RequestMethod.GET);

            dynamic template = await requestSender.SendRequestAsync($"https://fastreport.cloud/api/rp/v1/Templates/Folder/{templateRootFolder.id}/File",
                CloudRequestSender.RequestMethod.POST, boxFileVM);

            dynamic export = await requestSender.SendRequestAsync($"https://fastreport.cloud/api/rp/v1/Templates/File/{template.id}/Export", CloudRequestSender.RequestMethod.POST,
                new ExportVM() { fileName = "exportedByScript.pdf", format = "Pdf" });
            
            await requestSender.SendRequestAsync($"https://fastreport.cloud/download/e/{export.id}",
                CloudRequestSender.RequestMethod.GET);

            await requestSender.SendRequestAsync($"https://fastreport.cloud/api/rp/v1/Templates/File/{template.id}",
                CloudRequestSender.RequestMethod.DELETE);
            
            await requestSender.SendRequestAsync($"https://fastreport.cloud/api/rp/v1/Exports/File/{export.id}",
                CloudRequestSender.RequestMethod.DELETE);
        }
    }

    /// <summary>
    /// This is a view model for files, required to upload
    /// (you can also just provide JSON, without such representation)
    /// </summary>
    public struct FileVM
    {
        // file name have to include format (.frx to templates, .fpx to reports)
        public string name;
        // you can add tags to any file you upload
        public string[] tags;
        // you can select custom image as file's icon
        public string icon;
        // this is the base64 representation of file's content
        public string content;
    }

    /// <summary>
    /// This is a view model for export configuration
    /// </summary>
    public struct ExportVM
    {
        // you can configure export parameters here, if you want
        public string[] exportParameters;
        // file name (including export format)
        public string fileName;
        // folder for created export (root folder for exports by default)
        public string folderId;
        // export format
        public string format;
        // you can configure report parameters here, if you want
        public string reportParameters;
    }
}
