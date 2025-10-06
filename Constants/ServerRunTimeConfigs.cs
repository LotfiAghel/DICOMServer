using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.IO;
using System.Reflection;

using DnsClient;

namespace SGS.Core
{
    public enum ServerType
    {
        ROOMS = 1,
        USER_META = 2,
        MATCH_META = 3,
        SESSIONS = 4,

    }
    public class ServerRunTimeConfigs
	{

        public enum ServerStatus
        {
            Starting = 0,
            Ready = 1,
            ReceivedTermSignal = 2,
            MigratingUsers = 3,
            MigratingRooms = 4,
            MigratingMeta = 5,
            WaitingForSendQueues = 6,
            ShuttingDown = 7
        }

        public delegate Task InternalPublishHandlerDelegate(string channel, object data);
		

		public const int checkInterval = 1; //s
        public const int pingInterval = 10; //s
        public const int idleTimeout = (int)(pingInterval * 2.5); //s
        public const int pingTimeout = 3; //s
        public const int sessionTimeout = 5 * 60; //s
	        		
        public static Action OnServerStatusChanged;


		public static string serverType="ALL";

        public static string ExternalAddress;

        public static HashSet<ServerType> serverTypes=new HashSet<ServerType>();

        static ServerStatus _currentStatus = ServerStatus.Starting;
        public static ServerStatus CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                try
                {
                    OnServerStatusChanged();
                }
                catch (Exception ex)
                {
                    //Logging.Error("SCSServer", ex + ""); TODO fix this
                }
            }
        }

		public static string RootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static ServerRunTimeConfigs Instance;



		

		
		
		

		

       

		
	
	

		public static string GetResourcePath(params string[] path)
		{
			var arr = new string[path.Length + 1];
			arr[0] = Path.Combine(RootDir, "Resources");
			Array.Copy(path, 0, arr, 1, path.Length);
			return Path.Combine(arr).Replace(@"\", "/");
		}

		

		

	
	

		

        
  }
}