using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AdminServer.Services;
using Data.Data;
using EnglishToefl.Data;
using Data.Migrate;
using System;
using Services;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AdminServer;
using Models;
using AdminMsgs;
using EnglishToefl.Services;
using EnglishToefl.Auth;
using EnglishToefl.Helper;
using EnglishToefl.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using ClientMsgs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebApplication.Services;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Repositoris.AdminReps;
using SGS.Core;
using ServiceBuy = Models.ServiceBuy;

namespace WebApplication.Controllers
{
    [Route("[controller]")]
    public class AdminUserController : ControllerBase
    {


        private readonly DBContext _context;
        private readonly ApplicationDbContext _odb;


        IAdminUserService userService;
        public AdminUserController(DBContext context, ApplicationDbContext odb, IAdminUserService userService)
        {
            this._context = context;
            this._odb = odb;
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
                        var context2 = services.GetRequiredService<ApplicationDbContext>();
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
                        Issuer = "webApi",
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
        [HttpGet("migrateAllExamStart")]
        public async Task<object> migrateAllExam()
        {
            if (customers != null)
                return "start before";
            //customers = await _odb.Responses.OrderByDescending(x => x.EnterDate).GroupBy(x => x.CustomerId).Select(x => x.Key).ToListAsync();
            customers = await _odb.Customers.Select(x => x.id).ToListAsync();
            all = customers.Count();
            var zf = async (int of, int mod) =>
            {
                IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                for (int i = of; i < all; i += mod)
                {
                    if (DateTime.Now.Hour > 7)
                        return;
                    var uid = customers[i];
                    using (var scope = services2.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            var context = services.GetRequiredService<DBContext>();
                            var context2 = services.GetRequiredService<ApplicationDbContext>();
                            last = i;
                            var start = DateTime.UtcNow;
                            var user = await UserMigrator.MigrateUser(context2, context, context2.Customers.Find(uid));
                            await UserMigrator.migrateUserExamAndLeitner(context2, context, user, new UserMigrateState());
                            var userRamCacheDataRepository = new UserRamCacheDataRepository(context);
                            await userRamCacheDataRepository.createExamProgressBar(context, Guid.Parse(uid));
                            var time = DateTime.UtcNow - start;
                            times[uid] = time;
                            if (time > max)
                            {
                                max = time;
                                maxPhoneNumber = context.users.Find(Guid.Parse(uid)).phoneNumber;
                            }
                            sum += time;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("--");

                        }
                    }


                }

            };
            int n = 5;
            for (int i = 0; i < n; ++i)
            {
                var c = new Coroutine2(new AsyncContextThread());
                var x = i;
                _ = c.Run(async () => await zf(x, n));
            }
            //_ = Coroutine2.Instance2.Run(async () => await zf(1, 2));

            return $"{all}";


        }

        
        
        
        [HttpGet("migrateAllUser")]
        public async Task<object> migrateAllUser()
        {
            if (customers != null)
                return $"start before {last}/{all}";

            customers = await _odb.Customers.Select(x => x.id).ToListAsync();
            all = customers.Count();
            var zf = async (int of, int mod) =>
            {
                IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                for (int i = of; i < all; i += mod)
                {
                    var uid = customers[i];
                    using (var scope = services2.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            var context = services.GetRequiredService<DBContext>();
                            var context2 = services.GetRequiredService<ApplicationDbContext>();
                            last = i;
                            var start = DateTime.UtcNow;
                            var user = await UserMigrator.MigrateUser(context2, context, context2.Customers.Find(uid));
                            
                            var time = DateTime.UtcNow - start;
                            times[uid] = time;
                            if (time > max)
                            {
                                max = time;
                                maxPhoneNumber = context.users.Find(Guid.Parse(uid)).phoneNumber;
                            }
                            sum += time;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("--");

                        }
                    }


                }

            };
            int n = 3;
            for (int i = 0; i < n; ++i)
            {
                var c = new Coroutine2(new AsyncContextThread());
                var x = i;
                _ = c.Run(async () => await zf(x, n));
            }
            //_ = Coroutine2.Instance2.Run(async () => await zf(1, 2));

            return $"{all}";


        }

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
                        var context2 = services.GetRequiredService<ApplicationDbContext>();
                        last = i;
                        var uid = Customers[i];
                        
                        var user = await UserMigrator.MigrateUser(context2, context, context2.Customers.Find(uid));
                        await ExamMigrator.migrateServiceBuys(context2, context, Guid.Parse(uid));
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(new ExceptionMsg(ex.Message,ex.StackTrace));
                    }
                       
                }
                                            
            }
        }
        
        
        [HttpGet("migrateServiceBuys2")]
        public async Task<object> migrateServiceBuys2()
        {
            if (customers != null)
                return "start before";
            
            customers = await _odb.Customers.Select(x => x.id).ToListAsync();
            all = customers.Count();
            var zf = async (int of, int mod) =>
            {
                IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                for (int i = of; i < all; i += mod)
                {
                    //if (DateTime.Now.Hour > 7)
                    //    return;
                    var uid = customers[i];
                    using (var scope = services2.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            var context = services.GetRequiredService<DBContext>();
                            var context2 = services.GetRequiredService<ApplicationDbContext>();
                            last = i;
                            if (i == 0)
                            {
                                await Migrator.migrateTable<EnglishToefl.Models.ServiceBuy, Models.ServiceBuy, int, int>(context2.ServiceBuys, context.ServiceBuys);
                            }
                            var user = await UserMigrator.MigrateUser(context2, context, context2.Customers.Find(uid));
                            await ExamMigrator.migrateServiceBuys(context2, context, Guid.Parse(uid));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("--");
                            
                        }
                       
                    }
                    


                }
                customers = null;
                last = 0;

            };
            int n = 1;
            for (int i = 0; i < n; ++i)
            {
                var c = new Coroutine2(new AsyncContextThread());
                var x = i;
                _ = c.Run(async () => await zf(x, n));
            }
            //_ = Coroutine2.Instance2.Run(async () => await zf(1, 2));

            return $"{all}";


        }
        
        [HttpGet("migrateAllState")]
        public async Task<string> migrateAllState()
        {
            var s = $"{new DateTime(2024, 9, 25, 7, 0, 0) - DateTime.Now} {last}/{all}  ";
            foreach (var x in times)
                s += $"{x.Key} {x.Value}\n";
            return s;
        }
        [HttpGet("resetMigrateUser/{phoneNumber}/{examId}")]
        public async Task<string> resetMigrateUser([FromRoute] string phoneNumber, [FromRoute] int examId)
        {
            var oldUSer = await _context.users.Where(x => x.phoneNumber == phoneNumber).FirstOrDefaultAsync();
            await UserMigrator.migrateUserExam(_odb, _context, oldUSer, examId);
            
            return $"done";
        }


        [HttpGet("removeUserMigrateData/{phoneNumber}/{examId}")]
        public async Task<string> removeUserMigrateData([FromRoute] string phoneNumber, [FromRoute] int examId)
        {
            var oldUSer = await _odb.Customers.Where(x => x.UserName == phoneNumber).FirstOrDefaultAsync();
            var tmpn = await _context.usersMigrateData.Where(
                      x =>
                          x.user.phoneNumber == phoneNumber
                          && (x.enitityName == $"ExamPartSession-{examId}"
                                || x.enitityName == "migrateUserExamAndLeitner"
                                || x.enitityName == $"createLastScoreSession-{examId}"
                                || x.enitityName == $"Responses-{examId}"
                                || x.enitityName == $"fixSessionsScore-{examId}"
                                )
                          ).ExecuteDeleteAsync();
            return $"{tmpn}";
        }


        [HttpGet("removeUserMigrateDataLight/{phoneNumber}/{examId}")]
        public async Task<string> removeUserMigrateDataLight([FromRoute] string phoneNumber, [FromRoute] int examId)
        {
            var oldUSer = await _odb.Customers.Where(x => x.UserName == phoneNumber).FirstOrDefaultAsync();
            var tmpn = await _context.usersMigrateData.Where(
                      x =>
                          x.user.phoneNumber == phoneNumber
                          && (
                                   x.enitityName == $"migrateUserExam"
                                || x.enitityName == $"migrateUserExam-{examId}"
                                || x.enitityName == "migrateUserExamAndLeitner"
                                )
                          ).ExecuteDeleteAsync();
            return $"{tmpn}";
        }


        [HttpGet("removeStatisticsData/{uId}")]
        public async Task<string> removeStatisticsData([FromRoute] Guid uId)
        {
            //await _context.userSectionDatas.Where(x => x.CustomerId == uId).ExecuteDeleteAsync();
            //await _context.UserSubjectDatas.Where(x => x.CustomerId == uId).ExecuteDeleteAsync();
            //await _context.userExamPartDatas.Where(x => x.CustomerId == uId).ExecuteDeleteAsync();
            await _context.UserRamCacheDatas.Where(x => x.id == uId).ExecuteDeleteAsync();
            
            return $"done";
        }

        
        [HttpGet("MigrateAllOfUser/{userId}/{examId}")]
        public async Task<object> MigrateAllOfUser([FromRoute] Guid userId, [FromRoute] int examId)
        {
            return UserMigrator.migrateUserExamAndLeitnerStart(userId);

        }
        /*[HttpGet("reverseMigrate/{examId}")]
        public async Task<object> reverseMigrate([FromRoute] int examId)
        {
            await ExamMigrator.migrateReverce(_odb, _context);

            await _context.SaveChangesAsync();

            return new
            {
                done = true,
                sectionsPartsCount = _context.SectionParts.Where(x => x.Section.ExamId == examId).Count(),
                questionCounts = _context.Questions.Where(x => x.section.ExamId == examId).Count()
            };

        }*/


        [HttpGet("migrateUSer/{userId}")]
        public async Task<object> migrateUser([FromRoute] Guid userId)
        {
            var user = await _odb.Customers.FindAsync(userId.ToString());
            var nuser = await UserMigrator.MigrateUser(_odb, _context, user);
            UserMigrator.migrateUserExamAndLeitnerStart(nuser.id);
            //await UserMigrator.MigrateSession(_odb, _context, user);
            //await UserMigrator.MigrateResponses(_odb, _context, user);
            await _context.SaveChangesAsync();

            return new
            {
                done = true,
                examPartSessionsCount = nuser.examPartSessions.Count(),
            };

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


        [HttpPost("findWithTracker")]
        public async Task<ObjectContainer<Tracker>> findWithTracker([FromBody] Models.TSerach code)
        {
            code.Name=code.Name.Replace("\\", "/");
            return new ObjectContainer<Tracker>()
            {
                data = await _context.trackers.Where(x => x.name == code.Name).FirstOrDefaultAsync()
            };
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
            var sb=new ServiceBuy(_context.Services.Find(1),DateTime.UtcNow.AddYears(5),0)
            {
                TrackingNumber = 0,
                CustomerId = u.id,
                transactionCode="",
                moneyGateWay = MoneyGateWay.FREE,
            };
            _context.ServiceBuys.Add(sb);
            
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
        
        [HttpPost("calcFirstExams/{uId}")]
        public async Task<BooleanResponse> calcFirstExams([FromRoute] Guid uId)
        {

            var repo=new ExamStatisticsRepo(_context);
            
            
            
            return new BooleanResponse() { done = await repo.calcFirstExams(uId) };
        }
        
        
        [HttpPost("getUsers")]
        public async Task<GetUsersResponse> getUsers([FromBody] GetUsers code)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine(ModelState);
            }
            var user = await userService.check(Request);
            if (!user.roles.Contains(AdminUserRole.SUPER_USER))
                return null;

            return new GetUsersResponse()
            {
                number = await _context.users.CountAsync(x => x.examSessions.Any(x => x.startTime > code.range.start
                    && x.startTime < code.range.end

                    && (code.branchs == null || code.branchs.Contains(x.exam.company.learnBranch)))),
                numberWithEmail = await _context.users.CountAsync(x => x.examSessions.Any(x => x.startTime > code.range.start
                    && x.startTime < code.range.end
                    
                    && (code.branchs == null || code.branchs.Contains(x.exam.company.learnBranch)))
                    && x.email!=null
                ),
                
                    

            };
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
                        ResponseCount = _context.Responses.Count(),
                        ResponseSum = _context.Responses.Sum(x=> x.Score),
                        serviceBuys=_context.ServiceBuys.Count(),
                        serviceBuySums=_context.ServiceBuys.Sum(x=> x.PaidPrice)
                        
        
                    };
                }

    }
}