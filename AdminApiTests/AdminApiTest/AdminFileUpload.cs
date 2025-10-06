using ApiCall.Lab;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCall.AdminApiTest
{
    public  class AdminFileUpload
    {
        public static async Task sendFile(ClTool.WebClient webClient)
        {

            string path = "D:/downloads/ubuntu-20.04.1-desktop-amd64.iso";
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            //var uploadResults = await webClient.sendFile3("v1/file/f3", fs);

        }
    }
}
