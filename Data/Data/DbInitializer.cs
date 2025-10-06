using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Data.Data
{
    public static class DbInitializer
    {


       
        public static void Initialize(DBContext context)
        {
            //context.Database.EnsureCreated();
            context.Database.Migrate();
      





            //context.SaveChanges();
        }
    }
}