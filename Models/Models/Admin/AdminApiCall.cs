using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;


namespace Models.Admin
{
    
    

    [Display(Name = "محتوا")]
    [SelectAccess(AdminUserRole.DATA_ENTRY)]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    public class AdminApiCall : IdMapper<int>  
    {
        
        
        [ForeignKey(nameof(admin))]
        public Guid adminId { get; set; }
        
        [ForeignKey(nameof(adminId))]
        public AdminUser admin { get; set; }
        
        public string apiName { get; set; }
        public DateTime callDate { get; set; }
        

        [Column(TypeName = "jsonb")]
        public JToken data { get; set; }

    }
   



}
