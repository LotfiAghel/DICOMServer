using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Tools;
using Npgsql;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void manulConnection(IServiceCollection services)
        {
            var conn = new NpgsqlConnection("");
            
            //conn.TypeMapper.MapComposite<Rial>("Integer"); thats not work remove 7.0

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("ConfigureServices");


            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
           
            
            
            

            var dataSource2 = DBContext.createDataSource();

            
            //services.AddDbContext<SchoolContext>(options =>
            //    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<DBContext>(x =>
            {
                Console.WriteLine("services.AddDbContext<DBContext>");
                x.UseNpgsql(dataSource2);
                //x.UseSnakeCaseNamingConvention();

            });

            
            
            



            Console.WriteLine("ConfigureServices</>");

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine("Configure");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            Console.WriteLine("Configure</>");
            /*app.UseOpenApi();
                app.UseSwaggerUi3();/**/
        }
    }
}
