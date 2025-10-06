using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiCall.Lab
{
    public class AdminUserAPIs
    {

        public static async Task<bool> Login(ClTool.WebClient webClient)
        {
            var res = await webClient.fetch<AdminMsg.LoginRequest, AdminMsg.LoginResponse>("adminUser/login",
                           HttpMethod.Post, new AdminMsg.LoginRequest()
                           {
                               userName = "mina",
                               pass = "mina123"
                           });
            if (res == null || !res.done)
            {
                Console.WriteLine("not login");
                Console.WriteLine("res= " + (res != null ? JToken.FromObject(res) : "null"));
                return false;
            }
            Console.WriteLine("res= " + (res != null ? JToken.FromObject(res) : "null"));
            return true;
        }

    }
}
