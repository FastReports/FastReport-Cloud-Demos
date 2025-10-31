using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace UploadTemplateDemo
{
    public class CloudRequestSender
    {
        public enum RequestMethod { POST, GET, PUT, DELETE }
        private string apiKey;

        public CloudRequestSender(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<dynamic> SendRequestAsync(string request, RequestMethod method, object body = null)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");
            client.DefaultRequestHeaders.Add("accept", "text/plain");

            Console.WriteLine($"sending {method} request to {request}");
            HttpResponseMessage response;
            string json;
            StringContent data;
            switch (method)
            {
                case RequestMethod.GET:
                    response = await client.GetAsync(request);

                    if (response.Content.Headers.ContentType.MediaType == "application/octet-stream")
                    {
                        Console.WriteLine($"{method} request to {request} returned {response.StatusCode}");
                        if (response.StatusCode != HttpStatusCode.OK) return null;

                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fs = new FileStream("../../../../ExportedReport.pdf", FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                            {
                                await streamToReadFrom.CopyToAsync(fs);
                            }
                        }
                        return null;
                    }
                    break;
                case RequestMethod.POST:
                    if (body == null) throw new ArgumentException("You can't have body equal to null in POST request");
                    else if (body is MultipartFormDataContent multipartContent)
                    {
                        response = await client.PostAsync(request, multipartContent);
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(body);
                        data = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(request, data);
                    }
                    break;
                case RequestMethod.PUT:

                    if (body == null)
                    {
                        response = await client.PutAsync(request, null);
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(body);
                        data = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PutAsync(request, data);
                    }
                    Console.WriteLine($"{method} request to {request} returned {response.StatusCode}");
                    return null;
                case RequestMethod.DELETE:
                    response = await client.DeleteAsync(request);
                    Console.WriteLine($"{method} request to {request} returned {response.StatusCode}");
                    return null;
                default:
                    return null;
            }
            Console.WriteLine($"{method} request to {request} returned {response.StatusCode}");
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
