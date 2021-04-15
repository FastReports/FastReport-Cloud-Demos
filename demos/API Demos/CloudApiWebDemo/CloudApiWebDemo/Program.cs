using CloudApiWebDemo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

namespace CloudApiWebDemo
{
    public class Program
    {

        public static void Main(string[] args)
        {
            // we are using entity framework to make data storage simple and effective
            using (ApplicationContext db = new())
            {
                var books = db.Books.ToList();
                var json = JsonConvert.SerializeObject(books);
                using (StreamWriter w = new(Directory.GetCurrentDirectory() + @"/Resources/books.json"))
                {
                    w.WriteLine(json);
                }

            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


    }
}
