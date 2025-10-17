using Models;
using Newtonsoft.Json;
using System;


namespace Models
{
  public interface IUserEntity : IEntity
  {

        Guid userId{ get; set; }
        [JsonIgnore]
        User user{ get; set; }
  }
  public interface ISyncableEntity:IEntity3<Guid>
  {
        
  }
  public interface IEntity3<TKEY>:Models.IIdMapper<TKEY>  where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
  {
  }
  public interface IEntity : IEntity3<int>
  {
        
        
  }
    public interface IUserSyncableEntity : ISyncableEntity
  {

    Guid userId{ get; set; }
    [JsonIgnore]
    User user{ get; set; }
  }
}
