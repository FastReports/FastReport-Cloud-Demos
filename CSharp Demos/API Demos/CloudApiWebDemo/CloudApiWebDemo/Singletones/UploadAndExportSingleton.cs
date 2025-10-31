using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CloudApiWebDemo.Singletones
{
    public class UploadAndExportSingleton
    {
        public static string apiKey = Program.apiKey;
        public dynamic template;
        public dynamic pdfExport;
        public dynamic xlsxExport;
        private static bool initialized;

        public UploadAndExportSingleton()
        {
            initialized = false;
        }
        public async void Initialize()
        {
            if (!initialized)
            {
                template = await SendTemplateToCloud();
                pdfExport = await ExportTemplateToPDF((string)template.id);
                xlsxExport = await ExportTemplateToExcel((string)template.id);
                initialized = true;
            }
        }

        private static async Task<dynamic> SendTemplateToCloud()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");
            var request = await client.GetAsync(@"https://fastreport.cloud/api/rp/v1/Templates/Root");
            var folder = await request.Content.ReadFromJsonAsync<object>();
            var folderId = JsonConvert.DeserializeObject<dynamic>(folder.ToString()).id;

            string json = JsonConvert.SerializeObject(new
            {
                name = "pricelist.frx",
                tags = new string[] { "generated" },
                content = Convert.ToBase64String(System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + @"/Resources/PriceList.frx"))
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://fastreport.cloud/api/rp/v1/Templates/Folder/{folderId}/File", data);

            if (response.StatusCode != HttpStatusCode.OK) return null;
            var jsonResult = await response.Content.ReadFromJsonAsync<object>();
            return JsonConvert.DeserializeObject<dynamic>(jsonResult.ToString());
        }

        public async Task<dynamic> ExportTemplateToPDF(string templateId)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");

            string json = JsonConvert.SerializeObject(new
            {
                fileName = "pricelist.pdf",
                format = "Pdf"
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://fastreport.cloud/api/rp/v1/Templates/File/{templateId}/Export", data);

            if (response.StatusCode != HttpStatusCode.OK) return null;
            var jsonResult = await response.Content.ReadFromJsonAsync<object>();
            return JsonConvert.DeserializeObject<dynamic>(jsonResult.ToString());
        }

        public async Task<dynamic> ExportTemplateToExcel(string templateId)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");

            string json = JsonConvert.SerializeObject(new
            {
                fileName = "pricelist.xlsx",
                format = "Xlsx"
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://fastreport.cloud/api/rp/v1/Templates/File/{templateId}/Export", data);

            if (response.StatusCode != HttpStatusCode.OK) return null;
            var jsonResult = await response.Content.ReadFromJsonAsync<object>();
            return JsonConvert.DeserializeObject<dynamic>(jsonResult.ToString());
        }

        /// <summary>
        /// Simple method for converting plain text into base64 string
        /// </summary>
        /// <param name="plainText">plain text</param>
        /// <returns>base64 string</returns>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
