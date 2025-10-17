using Data.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Tools;


namespace ConsoleAdmin
{

    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger("Program");
            logger.LogInformation("Hello World! Logging is {Description}.", "fun");

            var ds = DBContext.createDataSource();
            services.AddDbContext<DBContext>(
                //x => x.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                x => {
                    x.UseNpgsql(ds).UseLoggerFactory(factory);
                }
                ); ;

            
          
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                SerializationBinder = TypeNameSerializationBinder.gloabl,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            settings.Converters.Add(new ForeignKeyConverter());
            settings.Converters.Add(new RialConverter());

            JsonConvert.DefaultSettings = () => settings;
            





            

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine("Configure");
            
            Console.WriteLine("Configure</>");
            
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        



    }
}