using CloudApiWebDemo.Models;
using CloudApiWebDemo.Singletones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CloudApiWebDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UploadAndExportSingleton uploadAndExport;
        public string apiKey = Program.apiKey;

        //application will upload template from resources folder and create export files on startup
        public HomeController(ILogger<HomeController> logger, UploadAndExportSingleton uploadAndExport)
        {
            _logger = logger;
            this.uploadAndExport = uploadAndExport;
            uploadAndExport.Initialize();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BookView(int id)
        {
            Book entity;
            using (ApplicationContext db = new())
            {
                entity = db.Books.Where(e => e.Id == id).FirstOrDefault();
            }
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookAsync(Book book)
        {
            using (ApplicationContext db = new())
            {
                db.Books.Add(book);
                await db.SaveChangesAsync();
                return Ok();
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            using ApplicationContext db = new();
            if (id == null)
            {
                return NotFound();
            }

            var movie = await db.Books.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            using ApplicationContext db = new();
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Books.Update(book);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<ActionResult> GetPDFPriceListAsync()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");

            await client.DeleteAsync($"https://fastreport.cloud/api/rp/v1/Exports/File/{uploadAndExport.pdfExport.id}");
            uploadAndExport.pdfExport = await uploadAndExport.ExportTemplateToPDF((string)uploadAndExport.template.id);

            bool done = false;
            bool success = false;
            while (!done)
            {
                var content = await (await client.GetAsync($"https://fastreport.cloud/api/rp/v1/Exports/File/{uploadAndExport.pdfExport.id}"))
                    .Content.ReadFromJsonAsync<object>();
                string status = JsonConvert.DeserializeObject<dynamic>(content.ToString()).status;
                switch (status)
                {
                    case "Success":
                        done = true;
                        success = true;
                        break;
                    case "Failed":
                        done = true;
                        break;
                }
            }
            if (success == true)
            {
                byte[] content;
                var response = await client.GetAsync($"https://fastreport.cloud/download/e/{uploadAndExport.pdfExport.id}");
                using (MemoryStream s = (MemoryStream)response.Content.ReadAsStream())
                {
                    content = s.ToArray();
                }

                return new FileContentResult(content, "application/pdf");
            }
            else
            {
                return BadRequest();
            }

        }

        // this tag is here for changing file name, and easy opening after downloading
        [HttpGet("Home/pricelist.xlsx")]
        public async Task<ActionResult> GetXlsxPriceListAsync()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode("apikey:" + apiKey)}");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloud API user");

            await client.DeleteAsync($"https://fastreport.cloud/api/rp/v1/Exports/File/{uploadAndExport.xlsxExport.id}");
            uploadAndExport.xlsxExport = await uploadAndExport.ExportTemplateToExcel((string)uploadAndExport.template.id);

            bool done = false;
            bool success = false;
            while (!done)
            {
                var content = await (await client.GetAsync($"https://fastreport.cloud/api/rp/v1/Exports/File/{uploadAndExport.pdfExport.id}"))
                    .Content.ReadFromJsonAsync<object>();
                string status = JsonConvert.DeserializeObject<dynamic>(content.ToString()).status;
                switch (status)
                {
                    case "Success":
                        done = true;
                        success = true;
                        break;
                    case "Failed":
                        done = true;
                        break;
                }
            }
            if (success == true)
            {
                byte[] content;
                var response = await client.GetAsync($"https://fastreport.cloud/download/e/{uploadAndExport.xlsxExport.id}");
                using (MemoryStream s = (MemoryStream)response.Content.ReadAsStream())
                {
                    content = s.ToArray();
                }

                return new FileContentResult(content, "application/octet-stream");
            }
            else return BadRequest();
        }

        public ActionResult GetJSONData()
        {
            using (ApplicationContext db = new())
            {
                return Ok(db.Books.ToList());
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

        #region Private_methods


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
        #endregion
    }
}
