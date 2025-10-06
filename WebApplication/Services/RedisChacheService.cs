using System;
using System.Reflection;
using System.Threading.Tasks;
using StackExchange.Redis;


namespace WebApplication.Services{
    public class RedisChache : IDisposable
    {
        private ConnectionMultiplexer connection;
        private IDatabase db;

        
      
        public void Setup()
        {
            // Pipelines.Sockets.Unofficial.SocketConnection.AssertDependencies();

            var options = ConfigurationOptions.Parse($"{Config.REDIS_HOST}:6379");
            connection = ConnectionMultiplexer.Connect(options);
            db = connection.GetDatabase(3);

        }

        void IDisposable.Dispose()
        {
            connection?.Dispose();
            db = null;
            connection = null;
        }
    }
}