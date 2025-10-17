using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Data.Data;
using Newtonsoft.Json;
using Npgsql;
using Models;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using Tools;
using WebApplication.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using WebApplication.Helpers;
using Microsoft.AspNetCore.Authentication;
using WebApplication.Controllers;




using NATS.Client;
using System.Linq;
using EnglishToefl.Services;
using EnglishToefl.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

using Microsoft.AspNetCore.Http;
using AdminPanel;
using System.Reflection;
using EnglishToefl.Data;
using Microsoft.Extensions.Logging;
using Services;
using Microsoft.Net.Http.Headers;
using Parbad.Builder;
using Parbad.Storage.EntityFrameworkCore.Builder;
using Parbad.Storage.EntityFrameworkCore.Context;
using ViewGeneratorBase;


namespace WebApplication
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
        public const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
        public readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public void olConfigure(IServiceCollection services)
        {
            ///JWT
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            //services.AddTransient<JwtFactory>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            //services.AddSingleton<ILoggerFactory>(factory);

            var ds = DBContext.createDataSource();
            services.AddDbContext<DBContext>(
                //x => x.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                x => { x.UseNpgsql(ds); }
            );
            ;

          
          

            services.AddScoped<Models.IAssetManager, DBContext>();
            //services.AddScoped<Models.IOldAssetManager, ApplicationDbContext>();

            services.AddScoped<Models.IMyServiceManager, MyServiceManager>();


            //services.AddDatabaseDeveloperPageExceptionFilter();

            DependesyContainer.serviceCollection = services;


            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                SerializationBinder = TypeNameSerializationBinder.gloabl,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            settings.Converters.Add(new ForeignKeyConverter());
            settings.Converters.Add(new RialConverter());
            settings.Converters.Add(new ForeignKeyConverter2());
            settings.Converters.Add(new ForeignKeyConverter3());
            
            

            JsonConvert.DefaultSettings = () => settings;
            //NpgsqlConnection.GlobalTypeMapper.UseJsonNet(null, null, settings);


            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                                "http://oncodraw.com",
                                "https://oncodraw.com",

                                "http://toefl.oncodraw.com",
                                "https://toefl.oncodraw.com",
                                "http://ielts.oncodraw.com",
                                "https://ielts.oncodraw.com",
                                "http://dev-acd.oncodraw.com",
                                "https://dev-acd.oncodraw.com",
                                
                                "http://fr.oncodraw.com",
                                "https://fr.oncodraw.com",
                                
                                "http://fr2.oncodraw.com",
                                "https://fr2.oncodraw.com",
                                "http://fr2.testboost.ir",
                                "https://fr2.testboost.ir",
                                "http://www.oncodraw.com",
                                "https://www.oncodraw.com",


                                "http://app.oncodraw.com",
                                "https://app.oncodraw.com",
                                
                                "http://iapp.oncodraw.com",
                                "https://iapp.oncodraw.com",
                                
                                "http://beta.oncodraw.com",
                                "https://beta.oncodraw.com",
                               
                               
                                "https://toefl-dev-acd.oncodraw.com",
                                "http://toefl-dev-acd.oncodraw.com",
                                "https://utrlmn.oncodraw.com",
                                "http://utrlmn.oncodraw.com",
                                "https://fortch.oncodraw.com",
                                "http://fortch.oncodraw.com",
                                "http://testboost.ir",
                                "https://testboost.ir",
                                
                                
                                "http://starnarimanovexams.com",
                                "https://starnarimanovexams.com",
                                 
                                "http://app.oncodraw.com",
                                "https://app.oncodraw.com",
                                
                                "http://iapp.oncodraw.com",
                                "https://iapp.oncodraw.com",
                                                
                                "http://oncodraw.com",
                                "https://oncodraw.com",
                                                
                                "http://www.oncodraw.com",
                                "https://www.oncodraw.com",

                                
                                "http://localhost:3001",
                                "https://localhost:3001",
                                "http://localhost:3000",
                                "https://localhost:3000",
                                "http://localhost",
                                "http://bxxrcoy7gwsramlmcholafv7mlsqbd24dciw3gul476sphv6pba3sryd.onion"
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
            //services.AddResponseCaching();
            services.AddControllers(
                    options => { options.Filters.Add(new AuthorizeFilter()); })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    options.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
                    options.SerializerSettings.SerializationBinder = TypeNameSerializationBinder.gloabl;
                    options.SerializerSettings.ContractResolver = MyContractResolver2.client2;
                    //options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;


                    options.SerializerSettings.Converters.Add(new RialConverter());
                    options.SerializerSettings.Converters.Add(new ForeignKeyConverter());
                    options.SerializerSettings.Converters.Add(new ForeignKeyConverter2());
                    options.SerializerSettings.Converters.Add(new ForeignKeyConverter3());
                });


            //services.AddControllersWithViews().AddNewtonsoftJson();
            //services.AddRazorPages().AddNewtonsoftJson();/**/


            //services.AddControllersWithViews();

          

            var ntypes = typeof(DBContext).GetProperties().Where(x =>
                    x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(y => y.PropertyType.GetGenericArguments()[0]).ToList()
                .Where(x => x.getKeyType() != null)
                .Select(
                    z => typeof(GenericClientCrudController<,,>).MakeGenericType(new Type[]
                        { z, z.getKeyType(), typeof(DBContext) })
                ).ToList();
            
            var rntype = new List<Type>()
            {
                
                typeof(Models.Notifications.News),
                typeof(Models.Notifications.UserInboxItem),
                
            };
            ntypes = rntype
                .Where(x => x.getKeyType() != null)
                .Select(
                    z => typeof(GenericClientCrudController<,,>).MakeGenericType(new Type[]
                        { z, z.getKeyType(), typeof(DBContext) })
                ).ToList();
            var n2Type = new Dictionary<Type, Type>();
            n2Type = rntype.ToDictionary(x => x,
                z => typeof(GenericClientCrudController<,,>).MakeGenericType(new Type[]
                    { z, z.getKeyType(), typeof(DBContext) }));
            
            
            
            
            olConfigure(services);

            services.AddSingleton<HttpContextAccessor>();
            //services.AddTransient<EnglishToefl.Services.AccountService>();
            //services.AddTransient<SyncService>();

            

            
            services.AddTransient<SMSService2>();

            services.AddMvc(o =>
                {
                    //o.Conventions.Add(new GenericClientControllerRouteConvention(typeof(GenericClientCrudControllerN<,>), "v1/generic/",  true));
                    
                    o.Conventions.Add(
                        new GenericClientControllerRouteConvention("v1/generic/", n2Type.Values.ToList(), true));
                }
            ).ConfigureApplicationPartManager(m =>
            {
                //m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider(typeof(GenericClientCrudControllerN<,>)));
                
                m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider(n2Type.Values.ToList()));
            });
            


            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = (new AuthorizationPolicyBuilder())
                    .RequireAuthenticatedUser()
                    .Build();
            });


            
            
            
            var ds2 = DBContext.createDataSource();
            /*pBuilder.ConfigureStorage(storage => storage.UseEfCore(options =>
                                                      {
                                                          const string connectionString = "Connection String";

                                                          // An Assembly where your migrations files are in it. In this sample the files are in the same project.
                                                          var migrationsAssemblyName = typeof(Program).Assembly
                                                                                                      .GetName()
                                                                                                      .Name;

                                                          options.ConfigureDbContext = db => db.UseNpgsql(ds2);

                                                            options.DefaultSchema = "Parbad";

                                                          //options.PaymentTableOptions.Name = "TABLE NAME";
                                                          //options.PaymentTableOptions.Schema = "SCHEMA NAME";

                                                          //options.TransactionTableOptions.Name = "TABLE NAME";
                                                          //options.TransactionTableOptions.Schema = "SCHEMA NAME";
                                                      }
                    )
            );/**/

            if (true)
            {
                services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

                // configure DI for application services
                services.AddScoped<IUserService, UserService>();
            }

            EntityFrameworkExtensions.serviceCollection = services;
            //EntityFrameworkExtensions.serviceCollection.AddScoped<IUserService, UserService>();
            //EntityFrameworkExtensions.serviceCollection.AddScoped<IUserServiceBase>(x=> new UsersBaseController(services.BuildServiceProvider().GetService<IUserService>()));
            services.AddHttpContextAccessor();
            //EntityFrameworkExtensions.serviceCollection.AddScoped<IUserServiceBase>(x=> new SomethingWithDependenciesOnContext(HttpContext.Current));
            //EntityFrameworkExtensions.serviceCollection.AddScoped<ResponeFilter, ResponeFilter>();
            //EntityFrameworkExtensions.serviceCollection.AddScoped<AsyncableEntiryUserQuery, AsyncableEntiryUserQuery>();

            {
                var ttyp1 = typeof(DBContext).GetProperties().Where(x =>
                        x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .Select(y => y.PropertyType.GetGenericArguments()[0]).ToList()
                    .Where(x => x.getKeyType() != null)
                    .Select(
                        z =>
                            z.GetCustomAttributes<Attribute>()
                                .Where(x => x.GetType().IsGenericType &&
                                            x.GetType().GetGenericTypeDefinition() == typeof(FroceFillter<>))
                                .Select(x => x.GetType().GenericTypeArguments[0]).ToList()
                    ).ToList();
                foreach (var x in ttyp1)
                {
                    foreach (var z in x)
                    {
                        EntityFrameworkExtensions.serviceCollection.AddScoped(z, z);
                    }
                }

                Console.WriteLine(ttyp1);
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            env.ContentRootPath = Environment.GetEnvironmentVariable("CONTENT_ROOT_PATH");
            JsonBase64File.UserUploadFolderPath = Environment.GetEnvironmentVariable("USER_UPLOAD_PATH")??"/userUpload";
            
            Config.externalUrl = Environment.GetEnvironmentVariable("external_url");
            Config.REDIS_HOST = Environment.GetEnvironmentVariable("REDIS_HOST")?? "localhost";;

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
            /*app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                                Path.Combine(env.ContentRootPath)),
                RequestPath = "/dictFiles"
            });/**/
            UserService.ConnectRedis();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                },
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "DictionariesStatisFiles")),
                RequestPath = "/dictFiles"
            });


            app.UseRouting();
            app.UseCors(x => x.WithOrigins(
                    "http://oncodraw.com",
                    "https://oncodraw.com",

                    "http://localhost:3001",
                    "https://localhost:3001",
                    "http://localhost:3000",
                    "https://localhost:3000",
                    "http://toefl.oncodraw.com",
                    "https://toefl.oncodraw.com",
                    "http://ielts.oncodraw.com",
                    "https://ielts.oncodraw.com",
                    "http://dev-acd.oncodraw.com",
                    "https://dev-acd.oncodraw.com",
                    
                    "http://fr.oncodraw.com",
                    "https://fr.oncodraw.com",
                    
                    "http://fr2.oncodraw.com",
                    "https://fr2.oncodraw.com",
                    "http://app.oncodraw.com",
                    "https://app.oncodraw.com",
                    "http://beta.oncodraw.com",
                    "https://beta.oncodraw.com",
                    "http://beta2.oncodraw.com",
                    "https://beta2.oncodraw.com",
                    "http://old.oncodraw.com",
                    "https://old.oncodraw.com",
                    "https://toefl-dev-acd.oncodraw.com",
                    "http://toefl-dev-acd.oncodraw.com",
                    "https://utrlmn.oncodraw.com",
                    "http://utrlmn.oncodraw.com",
                    "https://fortch.oncodraw.com",
                    "http://fortch.oncodraw.com",
                    "http://testboost.ir",
                    "https://testboost.ir",
                    
                    "http://starnarimanovexams.com",
                    "https://starnarimanovexams.com",

                    "http://fr2.testboost.ir",
                    "https://fr2.testboost.ir",
                    
                    "http://www.oncodraw.com",
                    "https://www.oncodraw.com",
                    
                     
            "http://app.oncodraw.com",
            "https://app.oncodraw.com",
                    
                    
            "http://iapp.oncodraw.com",
            "https://iapp.oncodraw.com",
                                                
            "http://oncodraw.com",
            "https://oncodraw.com",
                                                
            "http://www.oncodraw.com",
            "https://www.oncodraw.com",
            "http://localhost",
            "http://bxxrcoy7gwsramlmcholafv7mlsqbd24dciw3gul476sphv6pba3sryd.onion"

                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );


            {
                // DOCUMENT this block lines must be this order 
                // at first run  app.UseAuthentication()  and after that app.UseAuthorization()
                app.UseAuthentication();
                app.UseAuthorization();
            }

            //app.UseResponseCaching();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
            //dd.InitializeBot();
            /*app.UseOpenApi();
                app.UseSwaggerUi3();/**/
        }
    }
}