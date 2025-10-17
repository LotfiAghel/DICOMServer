using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace Models.Notifications
{


    
    public enum NotifType
    {
        NONE=0,
        success=1,
        info=2,
        warning=3,
        fail = 4,
    }

    public enum OnAriveMethod
    {
        NONE = 0,
        POPUP_IN_FIRST = 1,
        
    }
    [Display(Name = "اعلانات")]
    public class UserNotif : IEntity3<Guid>, IU,IRemoveable
    {
        [Key]
        public Guid id { get; set; }

      

        public NotifType notifType { get; set; }
        
        [Required]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "محتوی")]
        [DataType(DataType.MultilineText)]
        [RichText]
        public string Content { get; set; }


        [Display(Name = "فایلها")]
        [DataType(DataType.Upload)]
        [CollectionAttr(typeof(SmallPicShow))]
        public List<string> files { get; set; }

        public OnAriveMethod onAriveMethod { get; set; }

        
        public DateTime updatedAt { get; set; }
        public DateTime startDate { get; set; }

        [NotMapped]
        [JsonIgnore]
        [IgnoreDefultForm]
        public ChangeEventList onChanges { get; set; }
        public bool IsRemoved { get; set; } = false;

        public object getId()
        {
            return id;
        }
    }


    [Display(Name = "اعلانات")]
    [InsertAccess(AdminUserRole.SUPER_USER)]
    [UpdateAccess(AdminUserRole.SUPER_USER)]
    [SelectAccess(AdminUserRole.SUPER_USER)]
    public class News : UserNotif
    {
        [MultiSelect]
        public List<Platform> platforms { get; set; }
        [Display(Name = "برای کاربرای جدید نمایش داده شود؟")]
        public DateTime userRegisterMinDate { get; set; } = DateTime.UtcNow;
        public DateTime userRegisterMaxDate { get; set; } = DateTime.UtcNow;
        public DateTime endDate { get; set; }
        
        /*public List<PlatformBranch> platformBranchs { get; set; }
        public bool notClosable { get; set; } = false;
        public bool notLoginNeeded { get; set; } = false;
        public int ?minVersion { get; set; } = null;
        public int ?maxVersion { get; set; } = null;*/

    }
    
    [Display(Name = "پیام خصوصی")]
    [FroceFillter<AsyncableEntityUserQuery>(CustomIgnoreTag.Kind.CLIENT)]
    [InsertAccess(AdminUserRole.SUPER_USER)]
    [UpdateAccess(AdminUserRole.SUPER_USER)]
    [SelectAccess(AdminUserRole.SUPER_USER)]
    public class UserInboxItem : UserNotif, ICustomerEntity2
    {
        [ForeignKey(nameof(Customer))]
        public Guid CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public User Customer { get; set; }
    }

    public class AsyncableEntityUserQuery : IQuery2<ICustomerEntity2>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AsyncableEntityUserQuery(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IQueryable<T2> run<T2>(IQueryable<T2> q) where T2 : ICustomerEntity2
        {
            var user2Id=_httpContextAccessor.HttpContext.getUser2Id();
            //var userId=_httpContextAccessor.HttpContext.getUserId();
            
            return q.Where(x => x.CustomerId == user2Id //|| x.CustomerId == userId
            );

           
            
        }
    }


    [FroceFillter<AsyncableEntityUserQuery>(CustomIgnoreTag.Kind.CLIENT)]
    public interface ICustomerEntity2 
    {
        [ForeignKey(nameof(Customer))]
        public Guid CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public User Customer { get; set; }

    }




}
