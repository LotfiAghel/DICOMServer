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
using Models;

namespace AdminServer.Services
{
    public class AdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IAdminUserService _userService;


        public AdminAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAdminUserService userService
            )
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }
        public const string USER_COOKIE_KEY = "oUserId";
        public const string NEW_USER_COOKIE_KEY = "nUserId";
        public const string TOKEN_COOKIE_KEY = "ses";
        public const string SESSION_COOKIE_KEY = "token";


        public const string ITEM_USER_ID = "userId";
        public const string ITEM_NUSER_ID = "nUserId";
        public const string ITEM_Session_ID = "sessionId";



        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // skip authentication if endpoint has [AllowAnonymous] attribute
            
            Console.WriteLine("fetch :" + Request.Path);
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();

            string sessionId = "";
            AdminUser user = null;
            try
            {


                sessionId = Request.Cookies[Consts.ADMIN_SESSION_COOKIE_KEY];
                var password = Request.Cookies[Consts.ADMIN_TOKEN_COOKIE_KEY];

                try
                {
                    var userId = Guid.Parse(Request.Cookies[Consts.ADMIN_USER_COOKIE_KEY]);
                    user = await _userService.AuthenticateAdminSession(userId, sessionId, password);

                }
                catch
                {

                }

                
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");
            
            Context.Items[ITEM_NUSER_ID] = user.id;
            Context.Items[ITEM_Session_ID] = sessionId;

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.ToString()),
                //new Claim(ClaimTypes.Name, users.Username),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }



}
