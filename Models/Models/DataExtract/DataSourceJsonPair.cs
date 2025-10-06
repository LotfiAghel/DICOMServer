using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;

namespace Models.DataExtract;


[SelectAccess(AdminUserRole.SUPER_USER)]
[ViewAccess(AdminUserRole.SUPER_USER)]
[InsertAccess(AdminUserRole.SUPER_USER)]
[UpdateAccess(AdminUserRole.SUPER_USER)]

public class DataSourceJsonPair:IdMapper<Guid>
{
    [ForeignKey(nameof(clonedFrom))]
    public Guid ?clonedFromId { get; set; }
    
    public DataSourceJsonPair ?clonedFrom { get; set; }
    
    
    
    public string cloner { get; set; }
    
    [Column(TypeName = "jsonb")]
    public JToken source { get; set; }
    [Column(TypeName = "jsonb")]
    public JToken dst { get; set; }
    
    
    [Column(TypeName = "jsonb[]")]
    public List<object> neverFact { get; set; }
    
    [Column(TypeName = "jsonb[]")]
    public List<object> goodFact { get; set; }
    
    [InverseProperty(nameof(DataSourceJsonConvertor.clonedFrom))]
    public ICollection<DataSourceJsonConvertor> convertors { get; set; }
    
    
    
}


[SelectAccess(AdminUserRole.SUPER_USER)]
[ViewAccess(AdminUserRole.SUPER_USER)]
[InsertAccess(AdminUserRole.SUPER_USER)]
[UpdateAccess(AdminUserRole.SUPER_USER)]
public class DataSourceJsonConvertor:IdMapper<Guid>
{
  
    [ForeignKey(nameof(clonedFrom))]
    public Guid clonedFromId { get; set; }
    
    public DataSourceJsonPair clonedFrom { get; set; }
    
    [Column(TypeName = "jsonb")]
    public JToken convertor { get; set; }
   
   
}