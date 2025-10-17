using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiCall.Lab
{
    public class AdminUploadTest
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
        public static async Task uploadTest(ClTool.WebClient webClient)
        {
            string path = "C:\\Users\\f_l\\Downloads\\40da9a68-7eeb-40f9-94af-58d79bf0d3dc-image-DkyA7hng.png";

            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                byte[] b = new byte[1024 * 1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                string checksum = null;
                using (var md5 = MD5.Create())
                {
                    checksum = BitConverter.ToString(md5.ComputeHash(fs));
                }

                fs.Position = 0;
                //var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 1024));
                var tmp = await webClient.fetch<CreateSessionParams, SessionCreationStatusResponse>("api/file/create", HttpMethod.Post, new CreateSessionParams()
                {
                    FileName = "aa.aa",
                    dir="maslan2/maslan3",
                    dontChangeFileNameWithGUID=true,
                    forceWrite=true,
                    checkSum= checksum,
                    ChunkSize = 1000,
                    TotalSize = 10000
                });
                int x = 1;
                int bs = 0;
                while ( (bs= fs.Read(b, 0, b.Length)) > 0)
                {
                    await webClient.uploadFileSection("api/file/upload", tmp.SessionId, x++, b,bs);
                    //Console.WriteLine(temp.GetString(b));
                }
            }
        }


    }
}
