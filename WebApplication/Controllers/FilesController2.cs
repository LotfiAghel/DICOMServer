using EnglishToefl.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Data.Data;
using Models;
using ViewGeneratorBase;

namespace WebApplication.Controllers
{
    
    [AllowAnonymous]
    public class Files2Controller : UsersBaseController
    {
        private const string cacheZipFileDirectory = "../ZipCache";
        private const string dictinaryFileDirectory = "../Dicts";

        
        //private ILogger<FilesController> logger;
        
        private readonly IHostingEnvironment hostingEnvironment;
        private static readonly string[] SourceArray = new[] { ".jpg", ".jpeg", ".png", ".mp3", ".wav", ".ogg", ".mp4", ".m4v", ".pdf", ".doc", ".docx", ".rtf" };
        private static readonly string[] SourceArray0 = new[] { ".jpg", ".jpeg", ".png", ".mp3", ".wav", ".ogg", ".pdf", ".doc", ".docx", ".rtf" };

        public Files2Controller(
            IUserService us,
            IHostingEnvironment environment,
            DBContext dbContext 
            //ILogger<FilesController> logger
            ):base(us)
        {
            this.hostingEnvironment = environment;
            this.dbContext = dbContext;
            //this.logger = logger;

        }

        public DBContext dbContext { get; set; }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/upload")]
        public async Task<ActionResult> Upload(IFormFile file)
        {

            if (file == null)
                throw new Exception("No files sent...");

            var ext = Path.GetExtension(file.FileName);
            var fileName = file.FileName;
            if (fileName.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (fileName.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (fileName.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            if (!SourceArray0.Contains(ext))
                return BadRequest("Format Not Allowed");

            if (file.Length > 1024 * 1024) // 1000 KB
                return BadRequest("File Too Big");

            var userId = this.getUserId();
            //var uniqueFileName = GetUniqueFileName(file.FileName, userId);


            var uploadPath = "../userUpload/" + userId;
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
                stream.Close();
            }

            return Ok(new { filePath = fileName });
        }


        [HttpPost]
        [Authorize]
        [Route("api/[controller]/ticketingupload")]
        public async Task<ActionResult> TicketingUpload(IFormFile file)
        {
            if (file == null)
                throw new Exception("No files sent...");

            var ext = Path.GetExtension(file.FileName);
            if (!SourceArray.Contains(ext))
                return BadRequest("Format Not Allowed");

            if (file.Length > 5 * 1000 * 1000) // 5 MB
                return BadRequest("File Too Big");

            var userId = this.getUserId();
            var uniqueFileName = GetUniqueFileName(file.FileName, userId.ToString());

            var uploadPath = Path.Combine(hostingEnvironment.WebRootPath, "../../ticketing_uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, uniqueFileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
                stream.Close();
            }

            return Ok(new { filePath = uniqueFileName });
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/ticketingAttachment")]
        public ActionResult GetTicketingAttachment(string fileName)
        {
            if (!IsFilenameValid(fileName))
                return BadRequest();

            var path = Path.Combine(hostingEnvironment.WebRootPath, "../../ticketing_uploads");
            var filePath = Path.Combine(path, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = MimeTypeHelper.GetMimeType(Path.GetExtension(fileName));

            var res = File(System.IO.File.ReadAllBytes(filePath), contentType);
            res.EnableRangeProcessing = true;

            return res;
        }

        private string GetUniqueFileName(string fileName, string userId)
        {
            fileName = MakeValidFileName(Path.GetFileName(fileName));
            return userId + "_" +
                   Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }

        private string MakeValidFileName(string name)
        {
            var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private static bool IsFilenameValid(string name)
        {
            var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return !Regex.IsMatch(name, invalidRegStr);
        }

        [Route("api/[controller]/DownloadFile/{*Name}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadFile(string Name)
        {
            
            if (string.IsNullOrEmpty(Name))
                return BadRequest();
            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            //if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                //throw new SecurityException();
            if (Name.IndexOf('\\') > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UploadFolderPath, Name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            

            var file = new JsonBase64File(path);
            

            var contentType = getContentType(path);
            Response.Headers.Append("Cache-Control", "public,max-age=3600");
            var res = File(System.IO.File.ReadAllBytes(path), contentType);
            res.EnableRangeProcessing = true;
            return res;
        }

        public static string  getContentType(string path)
        {
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();
            return fileExtension switch
            {
                "wav" or "mp3" or "mpeg" or "ogg" => "audio/mpeg",
                "jpg" or "png" or "jpeg"=> "image/jpeg",
                "mp4" => "video/mp4",
                "apk" => "application/vnd.android.package-archive",
                _ => "txt"
            };
        }

        [Route("api/[controller]/DownloadFile2/{*name}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadFile2(string name)
        {

            if (string.IsNullOrEmpty(name))
                return BadRequest();
            if (name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            //if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                //throw new SecurityException();
            if (name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UploadFolderPath, name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }



            


            var contentType = getContentType(path);
            Response.Headers.Append("Cache-Control", "public,max-age=3600");
            var res = File(System.IO.File.ReadAllBytes(path), contentType);
            res.EnableRangeProcessing = true;
            return res;
        }
        /*Route("api/[controller]/DownloadFile2/public/{*name}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadFile3(string name)
        {

            if (string.IsNullOrEmpty(name))
                return BadRequest();
            if (name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            //if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                //throw new SecurityException();
            if (name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UploadFolderPath,"public", name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }



            
            var fileExtension = getContentType(path);
            Response.Headers.Append("Cache-Control", "public,max-age=3600");
            var res = File(System.IO.File.ReadAllBytes(path), contentType);
            res.EnableRangeProcessing = true;
            return res;
        }*/

        [Route("capi/[controller]/DownloadFile/{name}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadFileWithCookie(string name)
        {
            
           

            if (string.IsNullOrEmpty(name))
                return BadRequest();

            if (name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UploadFolderPath, name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var file = new JsonBase64File(path);
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();

            var contentType = getContentType(path);
            Response.Headers.Append("Cache-Control", "public,max-age=3600");
            var res = File(System.IO.File.ReadAllBytes(path), contentType);
            res.EnableRangeProcessing = true;
            return res;
        }



        [Route("api/[controller]/longman/{*name}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult longman(string name)
        {
            
           

            if (string.IsNullOrEmpty(name))
                return BadRequest();

            if (name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (name.IndexOf("~", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (name.IndexOf("/", StringComparison.Ordinal) == 0)
                throw new SecurityException();
            if (name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UploadFolderPath, $"longman/{name}");
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var contentType = getContentType(path);
           
            var res = File(System.IO.File.ReadAllBytes(path), contentType);
            res.EnableRangeProcessing = true;
            Response.Headers.Append("Cache-Control", "public,max-age=604800");
            return res;
        }

        [Route("api/[controller]/DownloadUserFile/{Name}")]
        [HttpGet]
        [Authorize]
        public ActionResult DownloadUserFile(string Name)
        {
            
           var userId = this.getUserId().ToString();
            if (string.IsNullOrEmpty(Name))
                return BadRequest();

            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UserUploadFolderPath, userId, Name);
            
            if (!System.IO.File.Exists(path))
            {
                userId=this.getUser2Id().ToString();//TODO security issue
            }
            
            path = Path.Combine(JsonBase64File.UserUploadFolderPath, userId, Name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var file = new JsonBase64File(path, userId);
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();

            var contentType = getContentType(path);
            var res = File(System.IO.File.ReadAllBytes(path), contentType, fileDownloadName: file.Title);
            res.EnableRangeProcessing = true;
            return res;
        }
        
        
        
        [Route("api/[controller]/DownloadUserFile2/{user2Id}/{Name}")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> DownloadUserFile2(Guid user2Id,string Name)
        {

            if (user2Id != getUserId())
            {
                var x=await _userService.getAcess(this.getUserId(),user2Id);
                if(x==false)
                    return BadRequest();
            }
            if (string.IsNullOrEmpty(Name))
                return BadRequest();

            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UserUploadFolderPath, user2Id.ToString(), Name);
            
           
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var file = new JsonBase64File(path,user2Id.ToString());
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();

            var contentType = getContentType(path);
            var res = File(System.IO.File.ReadAllBytes(path), contentType, fileDownloadName: file.Title);
            res.EnableRangeProcessing = true;
            return res;
        }
      

        [Route("api/[controller]/DownloadUserPublicFile/{userId}/{Name}")]
        [HttpGet]
        [Authorize]
        public ActionResult DownloadUserPublicFile(string userId,string Name)
        {
            
            var userId0 = this.getUserId().ToString();
            if (string.IsNullOrEmpty(Name))
                return BadRequest();

            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UserUploadFolderPath,"public", userId, Name);
            Console.WriteLine(path);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var file = new JsonBase64File(path, userId);
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();

            var contentType = getContentType(path);
            var res = File(System.IO.File.ReadAllBytes(path), contentType, fileDownloadName: file.Title);
            res.EnableRangeProcessing = true;
            return res;
        }

        [Route("api/[controller]/DownloadUserFile/{Name}/{UserId}")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DownloadUserFile(string Name,string UserId)
        {
            
            //string userId = User?.Claims?.FirstOrDefault(i => i.Type == "id")?.Value;
            if (string.IsNullOrEmpty(Name))
                return BadRequest();

            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(JsonBase64File.UserUploadFolderPath, UserId, Name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var file = new JsonBase64File(path, UserId);
            var fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();

            var contentType = getContentType(path);
            var res = File(System.IO.File.ReadAllBytes(path), contentType, fileDownloadName: file.Title);
            res.EnableRangeProcessing = true;
            return res;
        }

        [Route("api/[controller]/DownloadDictionary/{Name}")]
        [HttpGet]
        [Authorize]
        public ActionResult DownloadDictionary(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return BadRequest();
            if (Name.IndexOf("..", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("/", StringComparison.Ordinal) > 0)
                throw new SecurityException();
            if (Name.IndexOf("\\", StringComparison.Ordinal) > 0)
                throw new SecurityException();

            var path = Path.Combine(dictinaryFileDirectory, Name);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            return File(System.IO.File.ReadAllBytes(path), "application/octet-stream");
        }



        private static byte[] compressFiles(string[] files)
        {
            byte[] bytes;
            using (var packageStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        //Create a zip entry for each attachment
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string extenstion = Path.GetExtension(file).ToLower();
                        //if(extenstion.Contains("png")||extenstion.Contains("jpg"))
                        //{
                        //  fileName="."+fileName;
                        //  fileName=fileName.Substring(0,fileName.Length-1);
                        //}
                        var zipFile = archive.CreateEntry(fileName + extenstion);
                        using (var sourceFileStream = new MemoryStream(System.IO.File.ReadAllBytes(file)))
                        using (var zipEntryStream = zipFile.Open())
                        {
                            sourceFileStream.CopyTo(zipEntryStream);
                        }
                    }
                }

                bytes = packageStream.ToArray();
            }

            return bytes;
        }
        private static bool checkCacheForFile(string fileName)
        {
            //return false;
            var path = Path.Combine(cacheZipFileDirectory, fileName);
            if (!Directory.Exists(cacheZipFileDirectory))
                Directory.CreateDirectory(cacheZipFileDirectory);
            var lastModify = System.IO.File.GetLastWriteTime(path);
            if (lastModify < DateTime.Today.AddDays(-10))//created yesterday
                return false;
            if (System.IO.File.Exists(path))
                return true;
            return false;
        }

        private static byte[] readValidCacheFile(string fileName)
        {
            var path = Path.Combine(cacheZipFileDirectory, fileName);
            if (!checkCacheForFile(fileName))
                return null;
            return System.IO.File.ReadAllBytes(path);
        }

        private bool createCacheFile(string fileName, byte[] bytes)
        {
            var path = Path.Combine(cacheZipFileDirectory, fileName);
            try
            {
                System.IO.File.WriteAllBytes(path, bytes);
                return true;
            }
            catch (Exception e)
            {
                //logger.LogError(e, "Write File To Cache: " + e.Message);
                Console.WriteLine(e);
                return false;
            }


        }

    }
}