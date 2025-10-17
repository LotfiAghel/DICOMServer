using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Tools;
using System.Diagnostics;
using System.IO;

using Models;

/* cSpell:enable */
namespace WebApplication.Controllers
{

    [AllowAnonymous]
    [Route("public")]
    //[Route("[controller]")]
    public class PublicController : ControllerBase
    {
        DBContext _dbContex;
        static JsonSerializer serilizer;
         static PublicController()
        {
            
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                SerializationBinder = TypeNameSerializationBinder.gloabl,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            serilizer = JsonSerializer.Create(settings);
        }

        public PublicController(DBContext dBContext)
        {
            
            this._dbContex = dBContext;
            
        }

       


        [AllowAnonymous]
        [HttpGet("version")]
        public async Task<IActionResult> version()
        {
            
            return Ok(JToken.FromObject(new { version = Consts.gitVersion() }));//TODO
            

        }
        [AllowAnonymous]
        [HttpGet("gitversion")]
        public async Task<IActionResult> gitVersion()
        {
            return Ok(JToken.FromObject(new { version=Consts.gitVersion() }));//TODO


        }
        [AllowAnonymous]
        [HttpGet("gitversion2")]
        public async Task<IActionResult> gitVersion2()
        {
            StreamReader sr = new StreamReader("version.txt");
            //Read the first line of text
            var line = sr.ReadLine();
            sr.Close();
            
            return Ok(line);//TODO


        }
        public class ThifReq
        {
            public int idx { get; set; }
            public List<List<int>> volts { get; set; }
        }
        [AllowAnonymous]
        [HttpGet("thif")]
        public async Task<IActionResult> thif([FromBody] ThifReq req)
        {
            Console.WriteLine(JToken.FromObject(req).ToString());

            return Ok("ok");//TODO


        }


        [AllowAnonymous]
        [HttpGet("memUsage")]
        public async Task<IActionResult> memUsage()
        {
            Process proc = Process.GetCurrentProcess();
            System.GC.Collect();
            var memU = proc.PrivateMemorySize64;
            if (memU > 4000000000)
            {
                System.Environment.Exit(-1);
                //proc.ex
            }
            return Ok($"{memU}");//TODO


        }

        


        [AllowAnonymous]
        [HttpGet("time")]
        public async Task<IActionResult> time() => Ok(JToken.FromObject(new { date = DateTime.UtcNow }, serilizer));
        


        [AllowAnonymous]
        [HttpGet("health-check")]
        public async Task<IActionResult> healthCheck() => Ok(JToken.FromObject(new { date = DateTime.UtcNow ,version= $"{Config.GIT_COMMIT}-TODO",users= _dbContex.users.Count() }, serilizer));


        public class VersionCheck
        {
            public int minimumFlutter { get; set; }
            public List<string> iosUpgradeUrls { get; set; }
            public List<string> androidUpgradeUrls { get; set; }
            public int availableFlutter { get; set; }
        }

        [AllowAnonymous]
        [HttpGet("versionCheck")]
        public async Task<VersionCheck> versionCheck()
        {
            return new VersionCheck()
            {
                minimumFlutter=53,
                availableFlutter=53,
                iosUpgradeUrls=["link 1","link2"],
                androidUpgradeUrls=["https://oncodraw.com/toefl-api/api/files2/DownloadFile2/public/0/TestHelperToefl_1.1.63.apk"],
            };
        }


        
   
        
        /*
        
        [AllowAnonymous]
        [HttpGet("news")]
        public async Task<List<News> msgs([FromQuery]LearnBranch learnBranch,[FromQuery]Platform platform,[FromQuery] PlatformBranch platformBranch,[FromQuery] int  version)
        {
            return await _dbContex.news.Where(x => x.notLoginNeeded
                                             && !x.IsRemoved
                                             && x.startDate > DateTime.UtcNow
                                             && x.endDate < DateTime.UtcNow
                                             && (x.minVersion ==null || x.minVersion < version) 
                                             && (x.maxVersion ==null || x.maxVersion > version)
            ).ToListAsync();
        }

        */



    }
}