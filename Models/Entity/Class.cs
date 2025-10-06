using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;

namespace Models
{

    

   
    [GeneratedController]
    [PersianSmallDoc("جلسات کلاسها")]
    [PersianLabel("جلسه  ")]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class ClassSession : Id4Entity
    {

        //public Guid classId { get; set; }
        //public MyForeignKey<Guid, Class> Class { get; set; }


        


        
        public int lengthInSeconds { get; set; }


        public string title { get; set; }

        
        public DateTime? date { get; set; }

        public int? weekDay { get; set; }


        [SmallVideoShow]
        public string videoUrl { get; set; }

        
    }


    

}
