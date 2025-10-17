using AdminPanel;
using AdminServer.Services;

using Data.Data;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Tools;

using WebApplication.Controllers;



namespace AdminServer
{

    public class Startup
    {

        public const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            instance = this;
        }


        public static Startup instance;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            services.AddSingleton<ILoggerFactory>(factory);
            //Data.Startup.ConfigureServices(services);
            var ds = DBContext.createDataSource();
            services.AddDbContext<DBContext>(
                //x => x.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                x => {
                    x.UseNpgsql(ds).UseLoggerFactory(factory);
                }
                ); ;
            
            


            services.AddScoped<Models.IAssetManager, DBContext>((options) => options.GetRequiredService<DBContext>());


            services.AddScoped<Models.IMyServiceManager, MyServiceManager>();


            //services.AddDatabaseDeveloperPageExceptionFilter();




            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                SerializationBinder = TypeNameSerializationBinder.gloabl,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            
            settings.Converters.Add(new ForeignKeyConverter());
            settings.Converters.Add(new RialConverter());
            settings.Converters.Add(new ForeignKeyConverter2());
            settings.Converters.Add(new ForeignKeyConverter3());

            JsonConvert.DefaultSettings = () => settings;
            




            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                  builder =>
                  {
                      builder.WithOrigins("https://localhost:3000",
                                                "http://localhost:3000",

                                                "http://localhost:3020",
                                                "https://localhost:3021",




                                                 "https://admin.oncodraw.com",
                                                 "https://admin.oncodraw.com"







                                                ).AllowAnyMethod()
                                                .AllowAnyHeader().AllowCredentials()
                                                ;
                  });
            });

            /*services.AddMvc().AddNewtonsoftJson().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.
            }
            );/**/
            services.AddControllers(
                options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                }
                ).AddNewtonsoftJson(options =>
            {

                options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                options.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
                options.SerializerSettings.SerializationBinder = TypeNameSerializationBinder.gloabl;
                options.SerializerSettings.ContractResolver = MyContractResolver.client;
                //options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;





                options.SerializerSettings.Converters.Add(new RialConverter());
                options.SerializerSettings.Converters.Add(new ForeignKeyConverter());

                options.SerializerSettings.Converters.Add(new ForeignKeyConverter2());
                options.SerializerSettings.Converters.Add(new ForeignKeyConverter3());


            });

            //services.AddControllersWithViews().AddNewtonsoftJson();
            //services.AddRazorPages().AddNewtonsoftJson();/**/




            //services.AddControllersWithViews();

            
            var ntypes = typeof(DBContext).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(y => y.PropertyType.GetGenericArguments()[0]).ToList()
                .Where(x => x.getKeyType() != null)
                .Select(
                   z => typeof(GenericAdminCrudController<,>).MakeGenericType(new Type[] { z, z.getKeyType() })
               ).ToList();

            services.
                AddMvc(o =>
                {
                    //o.Conventions.Add(new GenericClientControllerRouteConvention( "", types));
                    //o.Conventions.Add(new AdminPanelOld.GenericClientControllerRouteConvention(typeof(AdminPanelOld.GenericAdminCrudController<,>), "", null));
                    
                    o.Conventions.Add(new GenericClientControllerRouteConvention("", ntypes, true));
                }
                ).
                ConfigureApplicationPartManager(m =>
                {
                    m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider(ntypes));
                    


                });


            services.AddAuthorization(options =>
            {
                //var policyBuilder = new AuthorizationPolicyBuilder();

                //options.DefaultPolicy = policyBuilder.RequireAuthenticatedUser().RequireClaim("rol", "api_access").Build();
                options.DefaultPolicy = (new AuthorizationPolicyBuilder())
                     .RequireAuthenticatedUser()
                     .Build();

                //options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });

            DependesyContainer.serviceCollection = services;

            if (true)
            {
                if(true)
                    services.AddAuthentication("BasicAuthentication")
                        .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>("BasicAuthentication", null);

                // configure DI for application services

                services.AddScoped<IAdminUserService, AdminUserService>();



            }
            

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            env.ContentRootPath = Environment.GetEnvironmentVariable("CONTENT_ROOT_PATH");
            var userUploadFolderPath = Environment.GetEnvironmentVariable("USER_UPLOAD_PATH");
            
            Config.externalUrl = Environment.GetEnvironmentVariable("external_url");

            Config.GIT_COMMIT = Environment.GetEnvironmentVariable("GIT_COMMIT");
            Console.WriteLine($"GIT_COMMIT = {Config.GIT_COMMIT}");

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
            //app.UseHttpsRedirection();
            //app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                                Path.Combine(env.ContentRootPath)),
                RequestPath = "/Upload"
            });/**/
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(userUploadFolderPath)),
                RequestPath = "/userUpload"
            });/**/



            app.UseRouting();
            app.UseCors(x => x.WithOrigins("https://localhost:3000",
                                                "http://localhost:3000",

                                                "http://localhost:3020",
                                                "https://localhost:3021",


                                                 "http://admin.oncodraw.com",
                                                "https://admin.oncodraw.com",
                                                "https://admin.oncodraw.com"

                                                                )
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            );


            {// DOCUMENT this block lines must be this order 
                // at first run  app.UseAuthentication()  and after that app.UseAuthorization()
                app.UseAuthentication();
                app.UseAuthorization();

                
            }
            



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
            });

            /*app.UseOpenApi();
                app.UseSwaggerUi3();/**/
        }



    }
   

}