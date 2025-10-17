using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using MoreLinq.Extensions;
using WebApplication.Services;
namespace WebApplication.Helpers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserService _userService;
        

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService
            )
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }
        public const string USER_COOKIE_KEY = "userId";
        public const string TOKEN_COOKIE_KEY = "token";
        public const string SESSION_COOKIE_KEY = "ses";


        public const string ITEM_USER_ID = "userId";
        public const string ITEM_USER2_ID = "user2Id";
        public const string ITEM_CONNECTION_ID = "connectionid";
        public const string ITEM_USER = "user";
        public const string ITEM_Session_ID = "sessionId";



        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // skip authentication if endpoint has [AllowAnonymous] attribute
            Console.WriteLine($"fetch : {Request.Path}");
            AuthenticateResult res = null;
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                res= AuthenticateResult.NoResult();

            if (Request.Path.ToString().Contains("getUser232"))
            {
                Console.WriteLine($"getUser232 : {Request.Path}");
                var cookieHeader = Request.Headers["Cookie"].ToString();
                Console.WriteLine(cookieHeader);
            }
            if (Request.Headers.TryGetValue("Cookie", out var rawCookie) && rawCookie.ToString().StartsWith("["))
            {
                // حذف براکت‌ها
                var raw = rawCookie.ToString().Trim('[', ']');

                // تبدیل به key=value فقط
                var parts = raw.Split(',', ';')
                    .Select(x => x.Trim())
                    .Where(x => x.Contains('=') && !x.Contains("expires", StringComparison.OrdinalIgnoreCase) &&
                                !x.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
                                !x.Contains("secure", StringComparison.OrdinalIgnoreCase) &&
                                !x.Contains("samesite", StringComparison.OrdinalIgnoreCase) &&
                                !x.Contains("path", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var cleaned = string.Join("; ", parts);

                Request.Headers["Cookie"] = cleaned;
            }
            string sessionId = "";
            Models.User user = null;
            try
            {
                sessionId = Request.Cookies[SESSION_COOKIE_KEY];
                var password = Request.Cookies[TOKEN_COOKIE_KEY];

                
                try{
                    if(Request.Cookies.TryGetValue(USER_COOKIE_KEY,out var nUserId))
                        user = await _userService.AuthenticateSession(Guid.Parse(nUserId), sessionId, password);
                    else
                        return res;
                }catch{
                    Console.WriteLine("--");
                }

                if (user != null)
                {
                    Context.Items[ITEM_USER_ID] = user.id;
                    Context.Items[ITEM_USER] = user;
                    Context.Items[SESSION_COOKIE_KEY] = Guid.Parse(sessionId);
                }
                StringValues nUserId2;
                string cookiecon,nUser2Idstr;
                Guid conId=Guid.Empty,usr2Id=Guid.Empty;
                if(Request.Headers.TryGetValue(ITEM_USER2_ID,out nUserId2))
                    usr2Id = Guid.Parse(nUserId2.ToString());
                else if(Request.Cookies.TryGetValue(ITEM_USER2_ID,out nUser2Idstr))
                    usr2Id = Guid.Parse(nUser2Idstr);
                
                if (Request.Headers.TryGetValue(ITEM_CONNECTION_ID, out nUserId2))
                    conId = Guid.Parse(nUserId2.ToString());
                else  if(Request.Cookies.TryGetValue(ITEM_CONNECTION_ID,out cookiecon))
                    conId = Guid.Parse(cookiecon);
                

                if (await _userService.getAcess(conId, user.id, usr2Id))
                {
                    Context.Items[ITEM_USER2_ID] = usr2Id;
                    Context.Items[ITEM_CONNECTION_ID] = conId;
                }
                
                //user2 = await _userService.AuthenticateSession(Guid.Parse(nUserId2), sessionId, password);

            }
            catch
            {

            }
        
            if (user == null)
                return res != null ? res : AuthenticateResult.Fail("Invalid Username or Password");
            
            Console.WriteLine($"fetch2 : {Request.Path} {user.id}");
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.ToString()),
                //new Claim(ClaimTypes.Name, users.Username),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return res != null ? res : AuthenticateResult.Success(ticket);
        }
    }


    
}