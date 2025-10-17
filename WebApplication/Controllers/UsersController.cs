using System;
using Models;
using Microsoft.AspNetCore.Http;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Microsoft.AspNetCore.Cors;
using StackExchange.Redis;

using System.IO;
using Microsoft.AspNetCore.Hosting;

using EnglishToefl.Data;
using Newtonsoft.Json.Linq;
using AdminPanel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EnglishToefl.Auth;
using ClientMsgs;
using EnglishToefl.Services;

using Google.Apis.Auth;
using ViewGeneratorBase;


//using static EnglishToefl.Services.AccountService;








namespace WebApplication.Controllers
{

    public class SomethingWithDependenciesOnContext : IUserServiceBase
    {
        HttpContext context;
        public SomethingWithDependenciesOnContext(HttpContext context)
        {
            this.context = context;
        }
        public System.Guid getUserId()
        {
            object res = null;
            if (context.Items.TryGetValue(WebApplication.Helpers.BasicAuthenticationHandler.ITEM_USER_ID, out res) && res is Guid)
                return (Guid)res;
            return Guid.Empty;

        }

    }

    [EnableCors]
    public class UsersBaseController : Controller, IUserServiceBase
    {
        internal IUserService _userService;
        //internal readonly DBContext _context;

        public UsersBaseController(IUserService userService)
        {
            //  this._context = context;
            _userService = userService;
        }




        public System.Guid getUserId()
        {
            return HttpContext.getUserId();

        }
        public System.Guid getUser2Id()
        {
            
            return HttpContext.getUser2Id();
            

        }
        
        
        public System.Guid getSesId()
        {
            object res = null;
            if (HttpContext.Items.TryGetValue(WebApplication.Helpers.BasicAuthenticationHandler.SESSION_COOKIE_KEY, out res) && res is Guid)
                return (Guid)res;
            return Guid.Empty;

        }
        public System.Guid getsessionPass()
        {
            object res = null;
            if (HttpContext.Items.TryGetValue(WebApplication.Helpers.BasicAuthenticationHandler.TOKEN_COOKIE_KEY, out res) && res is Guid)
                return (Guid)res;
            return Guid.Empty;

        }

        public string getSessionId()
        {

            return (string)HttpContext.Items[WebApplication.Helpers.BasicAuthenticationHandler.ITEM_Session_ID];
        }



        public async Task<Models.User> getNUser()
        {
            var uu = getUserId();

            return await _userService.getUser(uu);


        }

        public static Dictionary<string, string> sessionVerssion = new Dictionary<string, string>();
        public string getSessionClientVersion()
        {
            string ver = null;
            sessionVerssion.TryGetValue(getSessionId(), out ver);
            return ver;

        }
        public static void setCookie(HttpResponse Response, SessionMakeResult res)
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
                Expires = DateTime.UtcNow.AddDays(360),
                IsEssential = true
                //IsEssential = true,
                //Expires = DateTime.UtcNow.AddDays(60)
                //Domain = ".oncodraw.com",
            };

            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.USER_COOKIE_KEY, res.userId, cookieOptions);
            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.SESSION_COOKIE_KEY, res.sessionId, cookieOptions);
            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.TOKEN_COOKIE_KEY, res.password, cookieOptions);
        }

        


        public static void clearCookie(HttpResponse Response)
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
                Expires = DateTime.UtcNow.AddDays(360),
                //Domain = ".oncodraw.com",
                //IsEssential = true,
                //Expires = DateTime.UtcNow.AddDays(60)
            };

            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.USER_COOKIE_KEY,"", cookieOptions);
            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.SESSION_COOKIE_KEY, "", cookieOptions);
            Response.Cookies.Append(WebApplication.Helpers.BasicAuthenticationHandler.TOKEN_COOKIE_KEY,"", cookieOptions);
        }

    }


    [ApiController]
    [Route("v1/User")]
    [EnableCors]
    public class UsersController : UsersBaseController
    {

        private SMSService2 smsService;
        internal readonly DBContext _context;

        private readonly IWebHostEnvironment env;
        
        IUserService userService;
        public UsersController(DBContext context, SMSService2 smsService,  IUserService userService, IWebHostEnvironment env) : base(userService)
        {


            
            this._context = context;
            this.env = env;
            this.userService = userService;
            this.smsService = smsService;

            try
            {
                var options = ConfigurationOptions.Parse($"{Config.REDIS_HOST}:6379");
                connection = ConnectionMultiplexer.Connect(options);
                db = connection.GetDatabase(3);
            }
            catch
            {
                
            }

        }
        Random random = new Random((DateTime.UtcNow.Second + DateTime.UtcNow.Minute * 60 + DateTime.UtcNow.Hour * 60 * 60) * 1000 + DateTime.UtcNow.Millisecond);//TODO fix this bullshit


        //[LimitPerIp(TimeSpan.FromMinutes(1),5)]
        //[LimitPerUser(TimeSpan.FromMinutes(1),5)]

        static SortedSet<Tuple<DateTime, string>> ipOverTime = new SortedSet<Tuple<DateTime, string>>();
        static Dictionary<string, int> ipCount = new Dictionary<string, int>();
        



        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<string> Test()
        {

            //var done = await tools.SmsPanelClient.instance.SendSMS(code.phoneNumber, $"کد تایید جیم: \n\n{code.code}", false);
            var done = await smsService.SendOTPSmsAsync("09127903547", 1234); // true; //tools.SmsPanelClient.instance.SendSMS(code.phoneNumber, $"کد تایید جیم:  {code.code} ", false);



            return "salam";

        }




        [HttpPost("smsRequest")]
        [AllowAnonymous]
        public async Task<ClientMsgs.SmsRequestResponse> sendVerificationCode([FromBody] ClientMsgs.SmsRequest req)
        {
            string extraMsg = null;
            if (req.phoneNumber == "" || req.phoneNumber.Length != 11 || !req.phoneNumber.StartsWith("09"))
            {
                return new ClientMsgs.SmsRequestResponse()
                {
                    done = false,
                    extraMsg = "شماره تلفن اشتباه است"
                };
            }
            var ip = $"TODO{random.Next(0, 900)}";

            var tryNumber = await db.StringGetAsync($"smsRequest:{ip}");
            int x = 0;
            if (tryNumber.HasValue && false)
            {
                x = Int32.Parse(tryNumber);
                if (x > 10)
                {
                    return new ClientMsgs.SmsRequestResponse()
                    {
                        done = false,
                        extraMsg = "از ادرس شما درخواست زیادی داریم"
                    };
                }
                await db.StringIncrementAsync($"smsRequest:{ip}", 1);
            }
            else
            {
                await db.StringSetAsync($"smsRequest:{ip}", 1, TimeSpan.FromSeconds(60));
            }


            var code = await _context.verificationCodes.Where(x => x.createdAt > DateTime.UtcNow.AddMinutes(-10)
                && x.phoneNumber == req.phoneNumber).FirstOrDefaultAsync();
            if (code == null)
            {


                code = new VerificationCode()
                {
                    phoneNumber = req.phoneNumber,
                    code = random.Next(10000, 99999),
                    createdAt = DateTime.UtcNow
                };

                //var done = await tools.SmsPanelClient.instance.SendSMS(code.phoneNumber, $"کد تایید جیم: \n\n{code.code}", false);
                var done = await smsService.SendOTPSmsAsync(req.phoneNumber, code.code); // true; //tools.SmsPanelClient.instance.SendSMS(code.phoneNumber, $"کد تایید جیم:  {code.code} ", false);


                //await ks.SendSMS(code.phoneNumber, code.code.ToString());


                if (!done.Success)
                {
                    return new ClientMsgs.SmsRequestResponse()
                    {
                        done = false,
                        phoneNumber = code.phoneNumber,
                        extraMsg = !done.Success ? "سرویس در دسترس نیست با پشتیبانی تماس بگیرید" : null
                    };

                }
                _context.verificationCodes.Add(code);
                await _context.SaveChangesAsync();

            }
            else
            {
                extraMsg = "we sended token before , please use that";
                extraMsg = "ما قبلتر توکن ارسال کردیم ، لطفاً از آن استفاده کنید";
            }

            return new ClientMsgs.SmsRequestResponse()
            {
                done = true,
                phoneNumber = code.phoneNumber,
                codeId = code.id,
                secendsToExpire = (int)(code.createdAt.AddMinutes(5) - DateTime.UtcNow).TotalSeconds,
                secendsToNextSmsAvailabe = (int)(code.createdAt.AddMinutes(2) - DateTime.UtcNow).TotalSeconds,
                extraMsg = extraMsg
            };





        }


        [HttpPost("LoginByMobileVerifyCode")]
        [AllowAnonymous]
        public virtual async Task<ActionResult<OTPLoginRes>> LoginByMobileVerifyCode([FromBody] LoginWithOTP model)
        {
            if (model?.PhoneNumber?.StartsWith("+", System.StringComparison.InvariantCultureIgnoreCase) ?? false)
                model.PhoneNumber = model.PhoneNumber.Replace("+98", "0", System.StringComparison.InvariantCultureIgnoreCase);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var ip = model.PhoneNumber;

            var tryNumber = await db.StringGetAsync($"smscheck:{ip}");
            int x = 0;
            if (tryNumber.HasValue)
            {
                x = Int32.Parse(tryNumber);
                if (x > 6)
                {
                    return Ok(new OTPLoginRes() { done = false, text = "به دلیل تکرار زیاد کد دریافتی این شماره برای یک روز بلاک شده" });
                }
                await db.StringIncrementAsync($"smscheck:{ip}", 1);
                
            }
            else
            {
                await db.StringSetAsync($"smscheck:{ip}", 1, TimeSpan.FromSeconds(60*60*24));
            }
            
            var code = await _context.verificationCodes.Where(x => x.createdAt > DateTime.UtcNow.AddMinutes(-15)
          && x.phoneNumber == model.PhoneNumber && x.code == int.Parse(model.OTP)).FirstOrDefaultAsync();
            
            if (code == null)
            {
                //Console.WriteLine("sms TODO error");
                //Console.WriteLine()
                return Ok(new OTPLoginRes() { done = false, text ="  کد وارد شده صحیح نیست\n"+$"فقط {6-x} بار دیگر میتوانید کد وارد کنید"});
            }
            {
                
                Models.User user=null;
               
               user = await _context.users.Where(x => x.phoneNumber == model.PhoneNumber).FirstOrDefaultAsync();

                
                if (user == null)
                {
                    user = new User()
                    {
                        ServiceId = 1,
                        phoneNumber = model.PhoneNumber,
                        registerDate = DateTime.UtcNow,
                    };
                    await _context.users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    
                    
                }


                var tmp = await userService.MakeSession(user,model.platfrom);
                if (!tmp.done)
                {
                    return Ok(new OTPLoginRes() { done = false, text = " RDS 522", user = user });
                }

                setCookie(Response, tmp);
                var newRes = new OTPLoginRes() { done = true, text = "ثبت نام شما با موفقیت انجام شد", user = user };

                return Ok(newRes);
            }



            // If we got this far, something failed, redisplay form

        }

        private string DecodeJwtAndGetEmail(string jwt)
        {
            Console.WriteLine("jwt");
            Console.WriteLine(jwt);
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

            if (jsonToken == null)
                throw new ArgumentException("Invalid JWT");

            var email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;

            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email not found in JWT");

            return email;
        }
        private async Task<bool> ValidateGoogleJwtAsync(string jwt)
        {
            try
            {
                Console.WriteLine("jwt");
                Console.WriteLine(jwt);
                var payload = await GoogleJsonWebSignature.ValidateAsync(jwt);
                return true;
            }
            catch (InvalidJwtException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }
        [HttpPost("loginByGmail")]
        [AllowAnonymous]
        public virtual async Task<ActionResult<OTPLoginRes>> LoginByGmail([FromBody] LoginWithGmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Decode JWT and get email
                string email = DecodeJwtAndGetEmail(model.jwt);

                // Validate JWT with Google APIs
                bool isValidJwt = await ValidateGoogleJwtAsync(model.jwt);
                if (!isValidJwt)
                    return BadRequest("Invalid JWT");

                // Check if user exists
                Models.User user = await _context.users.FirstOrDefaultAsync(x => x.email == email);

                if (user == null)
                {
                    return Ok(new OTPLoginRes() { done = false, text = "کاربری با این ایمل وجود ندارد", user = user });
                    
                    // Create new user if not exists
                    user = new User()
                    {
                        ServiceId = 1,
                        phoneNumber = email,
                        email = email,
                        registerDate = DateTime.UtcNow,
                    };
                    await _context.users.AddAsync(user);
                    await _context.SaveChangesAsync();
                }

                // Create session
                var tmp = await userService.MakeSession(user, model.platfrom);
                if (!tmp.done)
                {
                    return Ok(new OTPLoginRes() { done = false, text = "RDS 522", user = user });
                }

                // Set cookie and return response
                setCookie(Response, tmp);
                var newRes = new OTPLoginRes() { done = true, text = "ثبت نام شما با موفقیت انجام شد", user = user };

                return Ok(newRes);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        
        
        [HttpPost("setByGmail")]
        public virtual async Task<ActionResult<User>>  setByGmail([FromBody] LoginWithGmail model)
        {
            var user = await _context.users.FindAsync(getUserId());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Decode JWT and get email
                string email = DecodeJwtAndGetEmail(model.jwt);
                email = email.ToLower();

                // Validate JWT with Google APIs
                bool isValidJwt = await ValidateGoogleJwtAsync(model.jwt);
                if (!isValidJwt)
                    return BadRequest("Invalid JWT");
                
                // Check if user exists

                if (!string.IsNullOrEmpty(user.email) && user.email.Length>4)
                    return BadRequest("برای این اکانت ایمیل قبلا ست شده");
                if(await _context.users.Where(x => x.email==email).AnyAsync())
                    return BadRequest("این ایمیل برای اکانت دیگیری قبلا ست شده");

                user.email = email;
                _context.users.Entry(user).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                

                return Ok(user);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
            
            
        }


        
        [HttpPost("login3rdParty")]
        [AllowAnonymous]
        public virtual async Task<ActionResult<OTPLoginRes>> login3rdParty([FromBody] Login3rdParty model)
        {
            if(String.IsNullOrEmpty(model.pass) || model.pass.Length<8)
                return BadRequest("password is too short");
            
            Models.User user = await _context.users.Where(x => x.phoneNumber.ToLower() == model.userName.ToLower() && x.pass == model.pass).FirstOrDefaultAsync();
            
            
            var extraData= await _context.userExtraDatas.Where(x => x.id == this.getUserId()).FirstOrDefaultAsync();
            if (extraData == null)
            {
                extraData = new UserExtraData() { id = this.getUserId() ,language = "en"};
                _context.userExtraDatas.Add(extraData);
                await _context.SaveChangesAsync();
            }


            

                if (user == null)
                    return BadRequest("user or password is wrong");
                var tmp = await userService.MakeSession(user,Platform.WEB);
                if (!tmp.done)
                    return Ok(new OTPLoginRes() { done = false, text = " RDS 522", user = user });


                setCookie(Response, tmp);
                var newRes = new OTPLoginRes() { done = true, text = "ثبت نام شما با موفقیت انجام شد", user = user };

                return Ok(newRes);


        }
        
        
      


        static TimeZoneInfo tz = TimeZoneInfo.Local;


      

        private ConnectionMultiplexer connection;
        private IDatabase db;








        static Dictionary<string, TaskCompletionSource<System.Guid>> tokensTasks = new Dictionary<string, TaskCompletionSource<System.Guid>>();

        





        public class JwtReQ
        {
            public string jwt { get; set; }
        }
        [HttpGet("loginByOldJwt/{jwt}")]
        [AllowAnonymous]
        public async Task<ClientMsgs.LoginResponse> loginByOldJwt([FromRoute] string jwt)
        {
            return await oldLogin0(new JwtReQ() { jwt = jwt });
        }
        [HttpPost("loginByOldJwt")]
        [AllowAnonymous]
        public async Task<ClientMsgs.LoginResponse> oldLogin0([FromBody] JwtReQ jwt)
        {

            var jwtFactory1 = new JwtFactory2(new JwtIssuerOptions()
            {
                Issuer = "asdfasdf",
                Audience = "http://localhost:5000/",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Startup.SecretKey)),
                    SecurityAlgorithms.HmacSha256),
            });
            var token = jwtFactory1.ValidateAndDecode(jwt.jwt);
            var userId = token.Claims.First(x => x.Type == "id").Value;
            var phoneNumber = token.Claims.First(x => x.Type == "sub").Value;
            Console.WriteLine(userId);
            User user;
            //TODO
           
            user = await _context.users.FindAsync(Guid.Parse(userId));
            if (String.IsNullOrEmpty(user.phoneNumber))
                                user.phoneNumber = phoneNumber;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            if (user == null)
            {
                user = await _context.users.Where(x => x.phoneNumber == phoneNumber).FirstOrDefaultAsync();
            }
            if (user == null)
            {

                user = new User()
                {
                    ServiceId = 1,
                    phoneNumber = phoneNumber,
                };
                await _context.users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            var x = getUserId();
            var ses = getSesId();
            var pass = getsessionPass();

            if (x == user.id )
            {
                /*var prvSession = new SessionMakeResult()
                {
                    done = true,
                    userId = x.ToString(),
                    sessionId = ses.ToString(),
                    password = pass.ToString()

                };
                setCookie(Response, prvSession);*/
                Console.WriteLine("us prv session ---------------");
                return new ClientMsgs.LoginResponse()
                {
                    done = true,
                    user = user,
                };
            }
            var tmp = await userService.MakeSession(user,Platform.ADMIN);
            setCookie(Response, tmp);
            Console.WriteLine("create new session ---------------");
            //var users=await _context.users.FindAsync(data.userId);
            return new ClientMsgs.LoginResponse()
            {
                done = true,
                user = user,
                sessionCookieData = tmp
            };
        }


        [HttpGet("logout")]
        public async Task<BooleanResponse> logout()
        {
            
            await userService.logout(await this.getNUser(),this.getSesId());
            clearCookie(this.Response);
            return new BooleanResponse(){done = true};
        }
        [HttpPost("setCookieIfValid")]
        [AllowAnonymous]
        public async Task<IActionResult> setCookieIfValid([FromBody] SessionMakeResult cookieFromClient)
        {
            if (cookieFromClient == null || string.IsNullOrEmpty(cookieFromClient.userId) 
                                         || string.IsNullOrEmpty(cookieFromClient.sessionId) || string.IsNullOrEmpty(cookieFromClient.password))
            {
                return BadRequest("Invalid session data.");
            }

            setCookie(HttpContext.Response, cookieFromClient);
            return Ok("Cookies set successfully.");
        }

        [HttpGet("getCookies")]
        public async Task<SessionMakeResult> getCookies()
        {
            var cookies = HttpContext.Request.Cookies;
            return new SessionMakeResult
            {
                userId = cookies.ContainsKey(WebApplication.Helpers.BasicAuthenticationHandler.USER_COOKIE_KEY) ? cookies[WebApplication.Helpers.BasicAuthenticationHandler.USER_COOKIE_KEY] : null,
                sessionId = cookies.ContainsKey(WebApplication.Helpers.BasicAuthenticationHandler.SESSION_COOKIE_KEY) ? cookies[WebApplication.Helpers.BasicAuthenticationHandler.SESSION_COOKIE_KEY] : null,
                password = cookies.ContainsKey(WebApplication.Helpers.BasicAuthenticationHandler.TOKEN_COOKIE_KEY) ? cookies[WebApplication.Helpers.BasicAuthenticationHandler.TOKEN_COOKIE_KEY] : null
            };
        }

        [HttpGet("getUser")]
        public async Task<ActionResult<ClientMsgs.GetUserResponse>> getUser0()
        {
            var nUser = await _context.users.FindAsync(getUserId());
            
            var rr=new ClientMsgs.GetUserResponse
            {
                user = nUser,
                //userExtraData = userExtraData,
                profile = await _context.profiles.FindAsync(getUserId()),
                
            };
            
         
            
            return Ok(rr);
        }
        public class SetSttingRequest
        {
            public string key { get; set; }
            public string value { get; set; }
        }
        
        [HttpPut("setSetting")]
        public async Task<ActionResult<UserExtraData>> setSetting([FromBody] SetSttingRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request." });

            if (string.IsNullOrWhiteSpace(request.key))
                return BadRequest(new { message = "Setting key cannot be empty." });
            var nUser = await _context.users.FindAsync(getUserId());
            
            
            var extraData= await _context.userExtraDatas.Where(x => x.id == this.getUserId()).FirstOrDefaultAsync();
            if (extraData == null)
            {
                extraData = new UserExtraData() { id = this.getUserId() };
                _context.userExtraDatas.Add(extraData);
                await _context.SaveChangesAsync();
            }

            if (request.key == nameof(UserExtraData.language))
                extraData.language = request.value;

            if (extraData.settings == null)
                extraData.settings = new Dictionary<string, string>();
            if (request.value != null)
                extraData.settings[request.key] = request.value;
            else
                extraData.settings.Remove(request.key);
            _context.Entry(extraData).State = EntityState.Modified;
            await _context.SaveChangesAsync();


            return Ok(extraData);
        }

        
        [HttpGet("getUser2/{userId}")]
        public async Task<ActionResult<ClientMsgs.GetUserResponse>> getUser2([FromRoute] Guid userId)
        {
            var nUser = await _context.users.FindAsync(userId);
            
            
           
            


            return Ok(new 
            {
                user = new
                {
                    nUser.id,
                    nUser.displayName,
                    nUser.avatar
                },
                //userExtraData = userExtraData,
                //profile = await _context.profiles.FindAsync(getUserId())
            });
        }

        


        [HttpGet("res/{propName}")]
        [IgnoreDocAttribute]
        public async Task<JToken> GetEntity(string propName, string range, string sort, string filter)
        {
            //if (!await checkPermission<ViewAccess>()) return Unauthorized(); //TODO check select access detail Entity
            var thisEntity = await getNUser();



            var range2 = range.convertToRange();
            dynamic thisDotPropertyValue = null;
            Type thisDotPropertyType = null;
            try
            {
                var thisDotProperty = typeof(User).GetProperty(propName);
                thisDotPropertyType = thisDotProperty.PropertyType;
                if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {



                    thisDotPropertyValue = _context.Entry(thisEntity)
                        .Collection(propName)
                        .Query();



                }

            }
            catch
            {

            }
            try
            {
                var thisDotProperty = typeof(User).GetMethod(propName);
                thisDotPropertyType = thisDotProperty.ReturnType;
                if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    thisDotPropertyValue = thisDotProperty.Invoke(thisEntity, new object[] { HttpContext.RequestServices });
                }
                if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    thisDotPropertyValue = thisDotProperty.Invoke(thisEntity, new object[] { HttpContext.RequestServices });
                }
            }
            catch
            {

            }
            var thisDotPropertyItemType = thisDotPropertyType.GetGenericArguments()[0];
            var total = Queryable.Count(thisDotPropertyValue);//TODO most add filter
            //thisDotPropertyValue = pp[propName](this, thisEntity);
            var sr = sort.convertToSort();
            thisDotPropertyValue = EntityFrameworkExtensions.addPagination(thisDotPropertyValue, range2, sr, filter);

            var x = await EntityFrameworkQueryableExtensions.ToListAsync(thisDotPropertyValue);


            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {thisDotPropertyItemType.Name}  {range2.Item1}-{range2.Item2}/{total}");
            //return JToken.FromObject(x, serializer);
            return JToken.FromObject(x, GenericClientCrudController.serializer);
            //return x;


        }
    

        
        [HttpPost("changeDisplayName")]
        public virtual async Task<ActionResult<User>> changeDisplayName([FromBody] ChangeDisplayName model)
        {
            if(model.name==null || model.name.Length>20)
                return BadRequest("is not ok");
            
            var thisEntity = await _context.users.FindAsync(getUserId());
            thisEntity.displayName = model.name;
            _context.Entry(thisEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return Ok(thisEntity);


        }
        [HttpPost("changeFirstLastName")]
        public virtual async Task<ActionResult<User>> changeFirstLastName([FromBody] ChangeDisplayName model)
        {
            if(model.name!=null || model.name.Length>20)
                return BadRequest("is not ok");
            
            var thisEntity = await _context.users.FindAsync(getUserId());
            thisEntity.firstAndLastName = model.name;
            _context.Entry(thisEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return Ok(thisEntity);


        }
        [HttpPost("uploadAvatar")]
        public async Task<ActionResult<User>> uploadAvatar([FromForm] UploadAvatar viewModel)
        {
            var uId = getUserId();
            var thisEntity = await _context.users.FindAsync(getUserId());
            var path = Path.Combine(JsonBase64File.UserUploadFolderPath,"public", uId.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            var fp = viewModel.File.FileName.Split(".").Last();
            if(fp!="jpg"  && fp!="png" && fp!="jpeg" )
                return ValidationProblem("jpg jpeg or png" );
            var name = $"{uId}.{fp}";
            
            if (viewModel.File?.Length > 0)
            {
                //large file
                if (viewModel.File?.Length > 5 * 1024 * 1024)
                {
                    return  ValidationProblem("Big file for avatar upload 5mb");
                }
                var fn = $"{path}/{name}";
                
                using (var stream = new FileStream(fn, FileMode.Create))
                {
                    await viewModel.File.CopyToAsync(stream);
                }
                
                thisEntity.avatar = $"{name}";
                _context.Entry(thisEntity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                await _context.SaveChangesAsync();
            }

            return thisEntity;
        }
        
       
     

   
        
     

    }

}