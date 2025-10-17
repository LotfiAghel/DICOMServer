using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AdminServer.Services;
using Data.Data;


using System;
using Services;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Models;
using AdminMsgs;
using EnglishToefl.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using ClientMsgs;
using EnglishToefl.Data;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using WebApplication.Services;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;


namespace WebApplication.Controllers
{
    [Route("[controller]")]
    public class AdminUserController : ControllerBase
    {


        private readonly DBContext _context;


        IAdminUserService userService;
        public AdminUserController(DBContext context,  IAdminUserService userService)
        {
            this._context = context;
            this.userService = userService;
        }
        

        public static async Task<object> GenerateJwtNotSerialized(ClaimsIdentity identity, JwtFactory2 jwtFactory,
            string userName, JwtIssuerOptions jwtOptions, TimeSpan? validityPeriod = null)
        {
            if (validityPeriod == null)
                validityPeriod = jwtOptions.ValidFor;
            var response = new
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = await jwtFactory.
                    GenerateEncodedToken(userName, identity, DateTime.UtcNow.Add(validityPeriod.Value)),
                expires_in = (int)(validityPeriod.Value.TotalSeconds)
            };

            return response;
        }
        [HttpGet("getSessions/{id}")]
        public async Task<GetSessionsResponse> getSessions(Guid id)
        {
            var sessions=await UserService.getSessions(id);
            var res = new GetSessionsResponse() { sessions = sessions.OrderBy(x=> x.lastUse).ToList() };
           

            return res;
        }

        private bool islock = false;
        [HttpGet("migrateServiceBuys")]
        public async Task<string> migrateServiceBuys()
        {
            IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
            if(islock)
                return "runing migrateServiceBuys";
            islock = true;
            var zf = async () =>
            {
                using (var scope = services2.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<DBContext>();
                        
                        //await ExamMigrator.migrateService(context2, context);
                        //await ExamMigrator.migrateServiceBuys(context2, context);
                    }
                    catch
                    {

                    }
                }
                this.islock = false;
            };
            var c = new Coroutine2(new AsyncContextThread());
            _ = c.Run(async () => await zf());

        return "ok";

    }
        
        
        [HttpGet("deleteAllSessions/{id}")]
        public async Task<int> deleteAllSessions(Guid id)
        {
            var m = RedisWrapper.server.Keys(database: 3, pattern: $"{id}:*:*");
            foreach (var l in m)
                RedisWrapper.db.KeyDelete(l);

            return m.Count();
        }
        
        [HttpGet("deleteSession/{userId}/{id}")]
        public async Task<int> deleteSession(Guid userId,Guid id)
        {
            RedisWrapper.db.KeyDelete($"{userId}:{id}:password");
            RedisWrapper.db.KeyDelete($"{userId}:{id}:platform");
            RedisWrapper.db.KeyDelete($"{userId}:{id}:lastUse");
            RedisWrapper.db.KeyDelete($"{userId}:{id}:createAt");
            return 0;
        }

        [HttpGet("getJwt/{phoneNumber}")]
        public async Task<GetJwtResponse> getJwt([FromRoute] string phoneNumber)
        {
            IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
            
            
                using (var scope = services2.CreateScope())
                {
                    var jwtFactory1 = new JwtFactory2(new JwtIssuerOptions()
                    {
                        Issuer = "asdf",
                        Audience = "http://localhost:5000/",
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Startup.SecretKey)),
                            SecurityAlgorithms.HmacSha256),
                    });

                    var userToVerify = _context.users.Where(x => x.phoneNumber == phoneNumber).First();
                    var services = scope.ServiceProvider;
                    //var jwtFactory1 = services.GetRequiredService<JwtFactory2>();
                    var identity = jwtFactory1.GenerateClaimsIdentity(phoneNumber, userToVerify.id.ToString());
                    var _jwtOptions = new JwtIssuerOptions();
                    _jwtOptions.ValidFor = new TimeSpan(365, 0, 0, 0);
                    dynamic jwt = await GenerateJwtNotSerialized(identity, jwtFactory1,
                        phoneNumber, _jwtOptions);

                    return new GetJwtResponse() { authToken = jwt.auth_token };

                }
            
           

            
        }


        public static List<string> customers = null;
        public static Dictionary<string, TimeSpan> times = new Dictionary<string, TimeSpan>();
        public static int all = 0;
        public static int last = 0;
        public static TimeSpan max = TimeSpan.FromSeconds(0);
        public static string maxPhoneNumber = "";
        public static TimeSpan sum = TimeSpan.FromSeconds(0);
       
        
        
     
        public class LongProcces
        {
            public class ExceptionMsg(string exMessage, string exStackTrace)
            {
                public string exMessage { get; set; }
                public string exStackTrace { get; set; }
            }
            internal List<string> Customers = null;
            public List<ExceptionMsg> Errors = new List<ExceptionMsg>();
            public int last { get; set; }
            public  async Task start(IServiceProvider services2)
            {
               
                
            }

            public async Task run(int i,IServiceProvider services2 )
            {
                using (var scope = services2.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<DBContext>();
                        
                        last = i;
                        var uid = Customers[i];
                        
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(new ExceptionMsg(ex.Message,ex.StackTrace));
                    }
                       
                }
                                            
            }
        }
        
     


       





        [HttpGet]
        [AllowAnonymous]
        public async Task<string> healthCheck2()
        {
            return Config.GIT_COMMIT;
        }
        [HttpGet("health-check3")]
        public async Task<string> healthCheck3()
        {
            return $"--{Config.GIT_COMMIT}";
        }

        [HttpGet("health-check")]
        [AllowAnonymous]
        public async Task<string> healthCheck()
        {
            return $"--{Config.GIT_COMMIT}";
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminMsg.LoginResponse>> login([FromBody] AdminMsg.LoginRequest code)
        {
            var adminUser = await _context.AdminUsers.Where(x => x.username == code.userName && x.password == code.pass).FirstOrDefaultAsync();
            if (adminUser == null)
            {
                return new AdminMsg.LoginResponse()
                {
                    done = false,
                    text = "wrong pass or userName"
                };
            }
            var res = await userService.MakeSession(adminUser);
            setCookie(Response, res);
            return new AdminMsg.LoginResponse()
            {
                done = true,
                text = "wellcome",
                user = adminUser
            };

        }


        [HttpGet("login/{userName}/{pass}")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminMsg.LoginResponse>> login0([FromRoute] string userName, [FromRoute] string pass)
        {
            var adminUser = await _context.AdminUsers.Where(x => x.username == userName && x.password == pass).FirstOrDefaultAsync();
            if (adminUser == null)
            {
                return new AdminMsg.LoginResponse()
                {
                    done = false,
                    text = "wrong pass or userName"
                };
            }
            var res = await userService.MakeSession(adminUser);
            setCookie(Response, res);
            return new AdminMsg.LoginResponse()
            {
                done = true,
                text = "wellcome",
                user = adminUser
            };

        }


        [HttpGet("getUser")]
        public async Task<ActionResult<AdminMsg.LoginResponse>> getUser()
        {
            try
            {
                var adminUser = await userService.check(Request);
                if (adminUser == null)
                {
                    return new AdminMsg.LoginResponse()
                    {
                        done = false,
                        text = "wrong pass or userName"
                    };
                }
                var res = await userService.MakeSession(adminUser);
                setCookie(Response, res);
                return new AdminMsg.LoginResponse()
                {
                    done = true,
                    text = "wellcome",
                    user = adminUser
                };
            }
            catch { return Unauthorized(); };

        }


     
     
        void setCookie(HttpResponse Response, AdminSessionMakeResult res)
        {
            var cookieOptions = new CookieOptions
            {
                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS
                Secure = true,

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                HttpOnly = true,

                // Add the SameSite attribute, this will emit the attribute with a value of none.
                // To not emit the attribute at all set the SameSite property to SameSiteMode.Unspecified.
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(60)
            };

            Response.Cookies.Append(Consts.ADMIN_USER_COOKIE_KEY, res.userId, cookieOptions);
            Response.Cookies.Append(Consts.ADMIN_SESSION_COOKIE_KEY, res.sessionId, cookieOptions);
            Response.Cookies.Append(Consts.ADMIN_TOKEN_COOKIE_KEY, res.password, cookieOptions);
        }

        [HttpPost("addUserWithEmail")]
        public async Task<ObjectContainer<User>> addUserWithEmail([FromBody] AddUserWithEmail code)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine(ModelState);
            }
            var user = await userService.check(Request);
            if (!user.roles.Contains(AdminUserRole.SUPER_USER))
                return null;
            
            _context.adminApiCalls.Add(new Models.Admin.AdminApiCall()
            {
                adminId = user.id,
                apiName = "AdminUserController::addUserWithEmail",
                data = JToken.FromObject(code),
                callDate = DateTime.UtcNow
            });

            var u=await _context.users.Where(x => x.email != null && x.email.ToLower() == code.email).FirstOrDefaultAsync();
            if (u != null)
                return new ObjectContainer<User>(u);
            u = new User()
            {
                phoneNumber =code.email.ToLower(), 
                email = code.email.ToLower(),
                registerDate = DateTime.UtcNow,
                pass = code.pass,
                ServiceId = 1,
                disableZarinPalBuying=code.disableZarinPalBuying
            };
            

            _context.users.Add(u);
            await _context.SaveChangesAsync();
            
            await _context.SaveChangesAsync();
            var extraData= await _context.userExtraDatas.Where(x => x.id == u.id).FirstOrDefaultAsync();
            if (extraData == null)
            {
                extraData = new UserExtraData() { id = u.id ,language = code.lang.ToString()};
                _context.userExtraDatas.Add(extraData);
                await _context.SaveChangesAsync();
            }
            return new ObjectContainer<User>(u); 
        }
        
        
        
        

        
        
        [HttpGet("checkDB")]
                public async Task<CheckDbResponse> checkDB()
                {
        
                    if (!ModelState.IsValid)
                    {
                        Console.WriteLine(ModelState);
                    }
        
                    return new CheckDbResponse()
                    {
                        userCount = _context.users.Count(),
                        
        
                    };
                }

    }
}