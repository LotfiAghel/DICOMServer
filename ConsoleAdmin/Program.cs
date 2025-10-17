using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Data.Data;




using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;



using Newtonsoft.Json.Linq;
using SGS.Core;


namespace ConsoleAdmin
{




    public class Program
    {
       
        
        
        public static IHost host;

        public static List<string> customers = null;

        public static async Task Main(string[] args)
        {
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ConsoleAdmin.Startup>();
                    //webBuilder.UseStartup<EnglishToefl.Data.Startup>();

                });
    }



}
