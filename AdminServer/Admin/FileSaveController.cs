using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using  System.Net.Mime;
using Microsoft.AspNetCore.Http.Features;
using AdminServer.Services;

using Data.Data;


public class UploadResult
{
    public bool Uploaded { get; set; }

    public string FileName { get; set; }

    public string StoredFileName { get; set; }

    public int ErrorCode { get; set; }
}
namespace AdminPanel
{
    [ApiController]
    [Route("v1/migrate")]
    public class MigrateController : ControllerBase
    {
        private readonly DBContext _db;
        

        IAdminUserService userService;
        public MigrateController(
             IAdminUserService userService,
             DBContext db
             )
        {
            this._db = db;
            this.userService = userService;
        }
        
    }
        [ApiController]
    [Route("v1/file")]
    [AllowAnonymous]
    public class FileSaveController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<FileSaveController> logger;
        
        IAdminUserService userService;
        public FileSaveController(IWebHostEnvironment env,
            ILogger<FileSaveController> logger, IAdminUserService userService)
        {
            this.env = env;
            this.logger = logger;
            this.userService = userService;
        }

        [HttpPost("pingUser")]
        [AllowAnonymous]
        public async Task pingUser()
        {
            string userId="bot";
            
            //await ClusterManager.runNotStatic2<IUserProccess,UserProccess>(userId, x => x.editField( new UserProccess.EditField()),"admin");
            
        }    
        [HttpPost("f2")]
        [AllowAnonymous]
        public void Save(IList<IFormFile> chunkFile, IList<IFormFile> UploadFiles)
        {
            long size = 0;
            try
            {
                foreach (var file in chunkFile)
                {
                    var filename = "a.a";
                    filename = env.ContentRootPath + $@"/{filename}";
                    size += file.Length;
                    if (!System.IO.File.Exists(filename))
                    {
                        using (FileStream fs = System.IO.File.Create(filename))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                    else
                    {
                        using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
                Response.Headers.Add("ID", "custom_ID"); // Assign the custom data in the response header. 
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }
        [HttpPost("f3")]
        [AllowAnonymous]
        public void Save2([FromForm] IFormFile file)
        {
            long size = 0;
            try
            {
                
                {
                    var filename = "a.a";
                    filename = env.ContentRootPath + $@"/{filename}";
                    size += file.Length;
                    if (!System.IO.File.Exists(filename))
                    {
                        using (FileStream fs = System.IO.File.Create(filename))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                    else
                    {
                        using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
                Response.Headers.Add("ID", "custom_ID"); // Assign the custom data in the response header. 
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }


        [HttpPost("[action]")]
        public void Remove(IList<IFormFile> UploadFiles)
        {
           
        }

        [HttpPost]
        public async Task<ActionResult<UploadResult>> PostFile(
            [FromForm] IEnumerable<IFormFile> files)
        {
            try { await userService.check(Request); } catch { return Unauthorized(); };
            var maxAllowedFiles = 1;
            long maxFileSize = 1024 * 1024 * 500;
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");


            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay =
                    WebUtility.HtmlEncode(untrustedFileName);

                if (filesProcessed < maxAllowedFiles)
                {
                    if (file.Length == 0)
                    {
                        logger.LogInformation("{FileName} length is 0",
                            trustedFileNameForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (file.Length > maxFileSize)
                    {
                        logger.LogInformation("{FileName} of {Length} bytes is " +
                            "larger than the limit of {Limit} bytes",
                            trustedFileNameForDisplay, file.Length, maxFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else
                    {
                        try
                        {
                            trustedFileNameForFileStorage = Path.GetRandomFileName() + trustedFileNameForDisplay;
                            var path = Path.Combine(env.ContentRootPath,
                                trustedFileNameForFileStorage);
                            using MemoryStream ms = new();
                            await file.CopyToAsync(ms);
                            await System.IO.File.WriteAllBytesAsync(path, ms.ToArray());
                            logger.LogInformation("{FileName} saved at {Path}",
                                trustedFileNameForDisplay, path);
                            uploadResult.Uploaded = true;
                            uploadResult.StoredFileName = trustedFileNameForFileStorage;
                        }
                        catch (IOException ex)
                        {
                            logger.LogError("{FileName} error on upload: {Message}",
                                trustedFileNameForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    logger.LogInformation("{FileName} not uploaded because the " +
                        "request exceeded the allowed {Count} of files",
                        trustedFileNameForDisplay, maxAllowedFiles);
                    uploadResult.ErrorCode = 4;
                }

                //uploadResults.Add(uploadResult);
                return new CreatedResult(resourcePath, uploadResult);
            }
            return new CreatedResult(resourcePath, null);

        }
    }

}