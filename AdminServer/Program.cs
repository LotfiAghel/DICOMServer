using Data.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer
{

    public class MyServiceManager : Models.IMyServiceManager
    {


      
        DBContext _db;

        public MyServiceManager( DBContext db)
        {

            _db = db;
        }


        public IHost host = null;
        public Models.IAssetManager getDB()
        {
            return _db;
        }

     


    }



    public class Program
    {
        public static IHost host;

        public static async Task Main(string[] args)
        {

            var host = CreateHostBuilder(args).Build();

            //Models.ServicesTool.services=host.Services;
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DBContext>();
                    DbInitializer.Initialize(context);


                    
                    //DbInitializer.Initialize(context2);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            await CreateDbIfNotExists(host);
            host.Run();
        }

        private static async Task CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DBContext>();
                    context.Database.Migrate();

                    
                    //context2.Database.Migrate(); I havent migrate permision in old DB
                    //await DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }


        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {

            var t = Environment.GetEnvironmentVariable("ADMIN_USE_URLS") ?? "https://localhost:3011;http://localhost:3010";
            Config.REDIS_HOST= Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            global::Config.REDIS_HOST = Config.REDIS_HOST;
            Console.WriteLine($"env USE_URLS={t}.");
            Console.WriteLine($"env REDIS_HOST={Config.REDIS_HOST}.");
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(t);
                });


        }
    }
}
