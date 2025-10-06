using System;
using Models;



using System.Collections.Generic;
using ClTool;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;
using CsvHelper;
using System.Globalization;
namespace ApiCall
{
    public class A
    {
        public class Foo
        {
            public string UC { get; set; }
            public string Name { get; set; }
            public string Tel { get; set; }

            public void refresh()
            {
                Tel = Tel.Replace(" ", "");
            }
        }

        public static void g()
        {
            var records = new List<Foo>
            {
                new Foo { UC = "", Name = "one",Tel="asdfa asdfad" },
                new Foo { UC = "", Name = "two" },
            };
            var absolutePath = "/home/lotfi/Downloads/emp_list2.csv";
            using (var writer = new StreamWriter(absolutePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }
        public static ClTool.WebClient webClient;
        
    }

}