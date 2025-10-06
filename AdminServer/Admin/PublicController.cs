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
using System.Reflection;
using Tools;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using EnglishToefl.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/* cSpell:enable */
namespace WebApplication.Controllers
{

    [AllowAnonymous]
    [Route("public2")]
    //[Route("[controller]")]
    public class PublicController : ControllerBase
    {
        DBContext _dbContex;
        ApplicationDbContext _oldContex;
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

        public PublicController(DBContext dBContext, ApplicationDbContext oldContex)
        {
            _oldContex = oldContex;
            this._dbContex = dBContext;
            
        }

       


        [AllowAnonymous]
        [HttpGet("version")]
        public async Task<IActionResult> version()
        {
            await _oldContex.Exams.AsNoTracking().ToListAsync();
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
        





    }
}