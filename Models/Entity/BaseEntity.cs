using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STR0 = System.String;
namespace Models
{
    using STR = System.String;


   
    public abstract class BaseEntity: IdMapper<System.Guid>
    {
  
        public virtual void refresh()
        {

        }
        

    }

}
