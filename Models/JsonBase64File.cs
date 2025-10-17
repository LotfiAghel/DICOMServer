using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ViewGeneratorBase
{
    public class JsonBase64File
    {
        public static List<string> GetFileNames(string baseFileName)
        {
            if (string.IsNullOrWhiteSpace(baseFileName))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(JsonBase64File.UploadFolderPath, $"{baseFileName}*");
            return files.ToList();
        }

        public static List<string> GetUserFileNames(string baseFileName, string uId)
        {
            if (string.IsNullOrWhiteSpace(baseFileName))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(JsonBase64File.UserUploadFolderPath + uId + "/", $"{baseFileName}*");
            return files.ToList();
        }

        public static List<JsonBase64File> GetFiles(string baseFileName)
        {
            if (string.IsNullOrWhiteSpace(baseFileName))
            {
                return new List<JsonBase64File>();
            }

            var files = Directory.GetFiles(JsonBase64File.UploadFolderPath, $"{baseFileName}*");
            return files.Select(x => new JsonBase64File(Path.GetFileName(x))).ToList();
        }

        public static List<JsonBase64File> GetUserFiles(string baseFileName, string uId)
        {
            var baseAddress = JsonBase64File.UserUploadFolderPath + uId + "/";
            if (string.IsNullOrWhiteSpace(baseFileName) || !Directory.Exists(baseAddress))
            {
                return new List<JsonBase64File>();
            }

            var files = Directory.GetFiles(baseAddress, $"{baseFileName}*");
            return files.Select(x => new JsonBase64File(Path.GetFileName(x), uId)).ToList();
        }



        public static void RemoveFiles(IEnumerable<JsonBase64File> removedFiles)
        {
            foreach (var file in removedFiles)
            {
                System.IO.File.Delete(Path.Combine(JsonBase64File.UploadFolderPath, file.Url.Split('/').Last()));
            }
        }



        /// <summary>
        /// Attention! not for user Files
        /// </summary>
        /// <param name="newFiles"></param>
        /// <param name="baseFileName"></param>
        /// <param name="startOrderFrom"></param>
        /// <returns></returns>
        public static async Task<bool> SaveNewFiles(List<JsonBase64File> newFiles, string baseFileName, int startOrderFrom = 0)
        {
            if (!Directory.Exists(JsonBase64File.UploadFolderPath))
            {
                Directory.CreateDirectory(JsonBase64File.UploadFolderPath);
            }

            if (newFiles == null)
                return false;
            for (int i = 0; i < newFiles.Count; i++)
            {
                //var contetnType = entity.Files[i].Src.Substring(0, entity.Files[i].Src.IndexOf(';'));
                //contetnType = contetnType.Substring(4);
                var nFile = newFiles[i].Src.Substring(newFiles[i].Src.IndexOf(','
                                                        , StringComparison.InvariantCulture) + 1);
                var fileBytes = Convert.FromBase64String(nFile);
                string filePath = JsonBase64File.UploadFolderPath + baseFileName + "^" + (i + startOrderFrom) + "^" +
                                  newFiles[i].Title;
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
            }

            return true;
        }


        public const string BaseDownloadUrl = "api/files/DownloadFile/";

        public const string BaseUserDownloadUrl = "api/files/DownloadUserFile/";

        public static string UploadFolderPath = "/Upload/";

        public static string UserUploadFolderPath = "/userUpload/";

        public JsonBase64File()
        {

        }

        public JsonBase64File(string fileName)
        {
            Url = BaseDownloadUrl + fileName;
            var parts = fileName.Split("^");
            Src = "NoChange";
            Title = parts.Last();
            Order = int.Parse(parts[1]);
            //ContectType = parts[2];
            ContectType = "application/octet-stream";
        }


        public JsonBase64File(string fileName, string uId)
        {
            Url = BaseUserDownloadUrl + fileName;
            var parts = fileName.Split("^");
            Src = "NoChange";
            Title = parts.Last();
            //ContectType = parts[2];
            if (Title.EndsWith(".mp3"))
            {
                ContectType = "application/octet-stream";
            }
            else
                ContectType = "application/octet-stream";
        }
        public string Src { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string ContectType { get; set; }

        public int Order { get; set; }
    }
}