using Models;
using Newtonsoft.Json;
using System;
using ViewGeneratorBase;

namespace Models
{
  public interface IUserEntity : IEntity
  {

        Guid userId{ get; set; }
        [JsonIgnore]
        User user{ get; set; }
  }

    public interface IUserSyncableEntity : ISyncableEntity
  {

    Guid userId{ get; set; }
    [JsonIgnore]
    User user{ get; set; }
  }
}
