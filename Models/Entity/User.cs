using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Models
{



    [PersianLabel("با تلفن")]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class ByPhone : IQuery<User>
    {
        public string phoneNumber { get; set; }
        public IQueryable<User> run(IQueryable<User> q)
        {
            return q.Where(x => x.phoneNumber == phoneNumber);
        }
    }

    [PersianLabel("قسمتی از تلفن")]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class LikePhone : IQuery<User>
    {
        public string phoneNumber { get; set; }
        public IQueryable<User> run(IQueryable<User> q)
        {
            return q.Where(x => EF.Functions.Like(x.phoneNumber, $"%{phoneNumber}%"));
        }
    }
    
    
    [PersianLabel("قسمتی از ایمیل")]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class LikeEmail : IQuery<User>
    {
        public string phoneNumber { get; set; }
        public IQueryable<User> run(IQueryable<User> q)
        {
            return q.Where(x => EF.Functions.Like(x.email.ToLower(), $"%{phoneNumber.ToLower()}%"));
        }
    }

    


    [PersianLabel("با شناسه")]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class ById : IQuery<User>
    {
        public Guid id { get; set; }
        public IQueryable<User> run(IQueryable<User> q)
        {
            return q.Where(x => x.id == id);
        }
    }
    [PersianLabel("مدیران ماک")]
    [SelectAccess(AdminUserRole.SUPPORT)]
    public class Managers : IQuery<User>
    {
        public string phoneNumber { get; set; }
        public IQueryable<User> run(IQueryable<User> q)
        {
            return q.Where(x => x.userType == UserType.MockThird);
        }
    }


    
  
   

   

    public class ListQuery<T> : IQuery<T>
    {
        public List<IQuery<T>> qs { get; set; }
        
        public IQueryable<T> run(IQueryable<T> q)
        {
            foreach (var t in qs)
                q = t.run(q);
            return q;
        }
    }
    
   

    public class DayI
    {
        public int y,m, d;
    }
    public class CC
    {
        public DateTime day { get; set; }
        public decimal PaidPrice { get; set; }
    }
    

    [GeneratedControllerAttribute]
    public class Master: IdMapper<Guid>
    {
        [InverseProperty(nameof(Detail.master))]
        public ICollection<Detail> details { get; set; }
    }

    
    
    
    [GeneratedControllerAttribute]
    public class Detail : IdMapper<Guid>
    {
        public Guid masterId { get; set; }

        [ForeignKey(nameof(masterId))]
        public Master master { get; set; }
    }

}
