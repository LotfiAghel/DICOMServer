using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Models;
namespace ClientMsgs
{
    public class ExtraMsgItem
    {

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
    public class ObjectContainerRespone<T> : BooleanResponse
    {
        public T data { get; set; }
        public List<ExtraMsgItem> items { get; set; }
        public ObjectContainerRespone() { }
        public ObjectContainerRespone(T data)
        {
            this.data = data;
        }
    }
    
    public class LongProccessRespone<T> : ObjectContainerRespone<T>
    {
        public int secondForTry { get; set; } = 0; 
        public LongProccessRespone() { }
        public LongProccessRespone(T data)
        {
            this.data = data;
        }
    }

    
    public class AvatarUploadResult : BooleanResponse
    {
        public string StoredFileName { get; set; }
        public Profile profile {get;set;}

    }
    public class UploadAvatar
    {



        


        public IFormFile File { get; set; }

        

    }
}