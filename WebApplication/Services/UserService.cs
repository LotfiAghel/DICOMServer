using System.Threading.Tasks;
using System;
using StackExchange.Redis;
using Models;
using System.Collections.Generic;
using System.Linq;
using Data.Data;
using Microsoft.EntityFrameworkCore;

using SGS.Core;
using ClientMsgs;
using System.Collections.Concurrent;
using AdminMsgs;


namespace WebApplication.Services
{




  

    
    public class RedisWrapper
    {


        private static ConnectionMultiplexer connection;
        public static IDatabase db;
        public static IServer server;
        static RedisWrapper()
        {
            var options = ConfigurationOptions.Parse($"{Config.REDIS_HOST}:6379");
            connection = ConnectionMultiplexer.Connect(options);
            db = connection.GetDatabase(3);
            server= connection.GetServer(Config.REDIS_HOST, 6379);
        }






    }
    public class UserService : IUserService, IDisposable
    {

        private static ConnectionMultiplexer connection=null;
        private IDatabase db;
        
        public DbSet<User> users;
        public DbSet<UserExaminer> tst;
        

        public static void ConnectRedis()
        {
            var options = ConfigurationOptions.Parse($"{Config.REDIS_HOST}:6379");
            try
            {
                connection = ConnectionMultiplexer.Connect(options);
            }
            catch (Exception e)
            {
                Console.WriteLine("redis error");
                Console.WriteLine(e.StackTrace);
            }
        }

        public UserService(DBContext context)
        {
            users = context.users;
            tst = context.userExaminers;

            if (connection == null)
                ConnectRedis();
            if(connection!=null)
                db = connection.GetDatabase(3);


        }

        void IDisposable.Dispose()
        {
            if (false)
            {
                connection?.Dispose();
                
                connection = null;
            }
            db = null;
        }
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        /*public async Task<User> AuthenticateSession(int userId, string sessionId, string password)
        {

            var password2 = await db.StringGetAsync($"{userId}:{sessionId}:password");
            if (password != password2)
                return null;
            return await users.FindAsync(userId);
        }/**/
        public async Task<int> logout(User user, Guid sessionId)
        {
            

            removeSession(user,sessionId);
            return 1;
        }

        public async Task<User> getUser(System.Guid userId)
        {
            if (!usersc.ContainsKey(userId))
            {
                var s=await users.Where(x => x.id == userId).AsNoTracking().FirstOrDefaultAsync();
                
                usersc[userId] = s;
            }

            return usersc[userId];
        }
        
        public async Task<bool> getAcess(System.Guid conectionId,Guid usId,Guid stId)
        {
            HashSet<Guid> ids;
            if(!EXTENCTION.techer2Student.TryGetValue(usId,out ids))
                EXTENCTION.techer2Student[usId] = ids = new HashSet<Guid>();
            if(ids.Contains(stId))
                return true;
            var z =await tst.Where(x=> x.id==conectionId && x.userId==stId && !x.IsRemoved && x.accepted).AnyAsync(); //TODO shud cash this becuse if z is false client can force query
            if(z)
                ids.Add(stId);
            return z;
        }
        public async Task<bool> getAcess(Guid usId,Guid stId)
        {
            HashSet<Guid> ids;
            if(!EXTENCTION.techer2Student.TryGetValue(usId,out ids))
                EXTENCTION.techer2Student[usId] = ids = new HashSet<Guid>();
            if(ids.Contains(stId))
                return true;
            var z =await tst.Where(x=> x.userId==stId && !x.IsRemoved && x.accepted).AnyAsync(); //TODO shud cash this becuse if z is false client can force query
            if(z)
                ids.Add(stId);
            return z;
        }
       
        static ConcurrentDictionary<System.Guid, Models.User> usersc = new ConcurrentDictionary<Guid, Models.User>();


       
        
        static ConcurrentDictionary<string, string> pass = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, SessionCacheData> cacheData = new ConcurrentDictionary<string, SessionCacheData>();
        

        public async Task<Models.User> AuthenticateSession(System.Guid userId, string sessionId, string password)
        {
            if (userId == System.Guid.Empty || sessionId == null)
                return null;
            //return  await getUser(userId);
            
            try
            {
                string password2, password3;
                if (!pass.TryGetValue($"{userId}:{sessionId}",out password2)){
                    password2 = await db.StringGetAsync($"{userId}:{sessionId}:password");
                   
                }
                if (!pass.TryGetValue($"{userId}:{password}", out password3))
                {
                    password3 = await db.StringGetAsync($"{userId}:{password}:password");
                   
                }

                
                
                if (password != password2 && password3 != sessionId)
                {
                    return null;
                    //Console.WriteLine($"[UserService] redis ok bot not ok {password}=={password2} or {sessionId}:{password3}");
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine("[UserService] redis error");
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
            }
            User user=null;
            try
            {
                user= await getUser(userId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[UserService] get user from DB error |{userId}|");
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
            }
            if (user == null)
            {
                Console.WriteLine("[UserService] get user from DB error [2]");
            }
            SessionCacheData cacheData2=await loadFromredis(userId, sessionId);
            cacheData2.lastUse = DateTime.UtcNow;
            if (cacheData2.lastSyncFromRedis.AddMinutes(2) < DateTime.UtcNow)
            {
                await syncWithRedis(userId, sessionId);
            }

           
            return user;
        }

        private async Task<SessionCacheData> syncWithRedis(Guid userId, string sessionId)
        {
            var ckey = $"{userId}:{sessionId}";
            string password2 = await db.StringGetAsync($"{userId}:{sessionId}:password");
            if (password2 == null)
            {
                cacheData.TryRemove(ckey, out SessionCacheData cacheData2);
                return null;
                
            }
            
            await db.StringSetAsync($"{userId}:{sessionId}:lastUse", cacheData[ckey].lastUse.totalMilliSeconds());
            cacheData[ckey].lastSyncFromRedis = DateTime.UtcNow;
            return cacheData[ckey];
        }

        private async Task<SessionCacheData> loadFromredis(Guid userId, string sessionId)
        {
            var ckey = $"{userId}:{sessionId}";
            if (!cacheData.TryGetValue(ckey, out SessionCacheData cacheData2))
            {
                cacheData[ckey] = cacheData2 = new SessionCacheData()
                {
                    lastSyncFromRedis     = DateTime.UtcNow
                };
            }

            return cacheData2;
        }


        public async Task<User> Authenticate(string phoneNumber, string password)
        {

            return await users.Where(x => x.phoneNumber == phoneNumber).FirstOrDefaultAsync(null);
        }
        Random random = new Random((int)(DateTime.UtcNow.totalMilliSeconds()));//TODO fix this bullshit

       
        public async Task<int> removeAnothers(Guid userId)
        {
            int res = 0;
            var server = connection.GetServer(Config.REDIS_HOST, 6379);
            foreach (var key in server.Keys(database:3, pattern: $"{userId}:*"))
            {
                res += 1;
               await db.KeyDeleteAsync(key);
            }
            return res;

        }
        public static async Task<List<SessionData>> getSessions(Guid id)
        {
            var m = RedisWrapper.server.Keys(database: 3, pattern: $"{id}:*:password");
            var sessions = new List<SessionData>();
            foreach (var x in m.ToList())
            {
                var sesId=Guid.Parse(x.ToString().Split(":")[1]);
                var lastUseKey=x.ToString().Replace(":password",":lastUse");
                var createAtKey=x.ToString().Replace(":password",":createAt");
                var platformKey=x.ToString().Replace(":password",":platform");

                var sss = RedisWrapper.db.StringGet(x);
                Guid.TryParse(sss.ToString(),out var pass);
                long.TryParse(RedisWrapper.db.StringGet(lastUseKey).ToString(),out var lastUseS);
                long.TryParse(RedisWrapper.db.StringGet(createAtKey).ToString(),out var createAtS);
                long.TryParse(RedisWrapper.db.StringGet(platformKey).ToString(),out var platformLong);
                
                
                
                sessions.Add(new SessionData()
                {
                    lastUse = lastUseS.milliSecToUtc(),
                    createAt = createAtS.milliSecToUtc(),
                    sessionId = sesId,
                    platform=(Platform)platformLong,
                    password = pass
                });
            }

            return sessions;
        }

        public void removeSession(User user,Guid sessionId)
        {
            var ckey = $"{user.id}:{sessionId}";
            cacheData.TryRemove(ckey, out _);
            pass.TryRemove(ckey,out _);
            
            RedisWrapper.db.KeyDelete($"{user.id}:{sessionId}:password");
            RedisWrapper.db.KeyDelete($"{user.id}:{sessionId}:platform");
            RedisWrapper.db.KeyDelete($"{user.id}:{sessionId}:lastUse");
            RedisWrapper.db.KeyDelete($"{user.id}:{sessionId}:createAt");
        }
        public async Task<SessionMakeResult> MakeSession(User user, Platform modelPlatform)
        {

            var sessionId = System.Guid.NewGuid().ToString();
            var password = System.Guid.NewGuid().ToString();
            var key = $"{user.id}:{sessionId}:password";
            //usersc[user.id] = user;
            var res=await getSessions(user.id);
            // حذف سشن‌ها به جز سه سشن با آخرین زمان استفاده
            if (res.Count(x => x.platform == modelPlatform) > 2)
            {
                var sessionsToRemove = res
                    .Where(x => x.platform == modelPlatform)
                    .OrderBy(x => x.lastUse) // مرتب‌سازی بر اساس زمان آخرین استفاده
                    .Take(res.Count(x => x.platform == modelPlatform) - 3) // انتخاب سشن‌هایی که باید حذف شوند
                    .ToList();

                foreach (var session in sessionsToRemove)
                {
                    removeSession(user, session.sessionId);
                }
            }
            if (db!=null)
            {
                try
                {
                    pass[key] = password;
                    
                    await db.StringSetAsync(key, password);
                    await db.StringSetAsync($"{user.id}:{sessionId}:createAt", DateTime.UtcNow.totalMilliSeconds());
                    await db.StringSetAsync($"{user.id}:{sessionId}:lastUse", DateTime.UtcNow.totalMilliSeconds());
                    await db.StringSetAsync($"{user.id}:{sessionId}:platform", (int)modelPlatform);
                }
                catch
                {
                    Console.WriteLine("redis error");
                }
            }
            return new SessionMakeResult()
            {
                sessionId = sessionId,
                password = password,
                userId = user.id.ToString(),
                done = true
            };

        }

    }

   

}
