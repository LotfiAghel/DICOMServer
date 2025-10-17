using System;
using Models;



using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AdminMsg;
using System.Linq;


namespace ApiCall
{

    class Program
    {

      
      
      
     
        public static async Task helthCheck(ClTool.WebClient lbackWebClient)
        {

            
            var uc = new PublicService(lbackWebClient);
            while (true)
            {
                try
                {

                    await uc.healthCheck();
                    break;

                }
                catch
                {

                }
            }
        }
        public static string lback = "https://admin-server.testhelper.ir/";
        public static async Task Main(string[] args)
        {

            //await GrammerAi.createAsistant();
            //string res;
            //res = await GrammerAi.AiTest2("i am go to the school and saw the my frieend and I said to him why you are there");
            // res = await GrammerAi.AiTest2("i will go to the school and saw the my frieend");
            //res = await GrammerAi.AiTest2(" You are an english llanguage editor who helps the user to write better and fix spelling and grammar mistakes.\r\n");


            //lback = "https://alfa-admin.oncodraw.com/";
            var cc = ClTool.WebClient.webClient = new ClTool.WebClient(lback);
            await helthCheck(cc);
            var tz = new AdminUserController(cc);
            await tz.login(new LoginRequest
            {
                userName = "machinAdmin",
                pass="dicom"
            });

            //await MemoryCheck(cc);
            
            //await tz.migrate();
            
       

            return ;
           




        }
    }
    
    
    
}
