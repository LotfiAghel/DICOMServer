using Data.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EnglishToefl.Data;

namespace WebApplication
{


    public class MyServiceManager:Models.IMyServiceManager{
        

        ApplicationDbContext _old;
        DBContext _db;

        public MyServiceManager(ApplicationDbContext old,DBContext db){
            _old=old;
            _db=db;
        }

        
        public IHost host=null;
        public Models.IAssetManager getDB(){
            return  _db;
        }

        /*public Models.IOldAssetManager getOldDb(){
            return _old;

        }*/


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
                    var context2 = services.GetRequiredService<ApplicationDbContext>();
                    /*await*/ DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

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
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            
            var t=Environment.GetEnvironmentVariable("USE_URLS");
            Console.WriteLine("env USE_URLS="+t+".");
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                     webBuilder.UseUrls(t);
                });

            
        }
    }
}