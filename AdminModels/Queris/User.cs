using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnglishToefl.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Models;

public class ByPhone2 : IQuery<Customer>
{
    public string phoneNumber { get; set; }
    public IQueryable<Customer> run(IQueryable<Customer> q)
    {
        return q.Where(x => x.PhoneNumber.Contains(phoneNumber));
    }
}

public class ByUserName : IQuery<Customer>
{
    public string userName { get; set; }
    public IQueryable<Customer> run(IQueryable<Customer> q)
    {
        return q.Where(x => EF.Functions.Like(x.UserName, $"%{userName}%"));
    }
}
[PersianLabel("قسمتی از تلفن")]
public class LikePhoneOldUSer : IQuery<Customer>
{
    public string phoneNumber { get; set; }
    public IQueryable<Customer> run(IQueryable<Customer> q)
    {
        return q.Where(x => EF.Functions.Like(x.PhoneNumber, $"%{phoneNumber}%"));
    }
}



[PersianLabel("قسمتی از تلفن")]
public class ServiceBuySearch : IQuery<ServiceBuy>
{
    public Range<DateTime> range{ get; set; }
    public IQueryable<ServiceBuy> run(IQueryable<ServiceBuy> q)
    {
        return q.Where(x =>x.BuyTime>range.start && x.BuyTime<range.end);
    }
}

public class MyClass
{
    public DateTime date { get; set; }
    public decimal PaidPrice { get; set; }
}
public class ServiceBuyGropByDay : IQuery<ServiceBuy,MyClass>
{
   
    public IQueryable<MyClass> run(IQueryable<ServiceBuy> q)
    {
        return q.GroupBy(x=> x.BuyTime.Date).Select(
                    x=> new MyClass(){
                        date=x.Key,
                        PaidPrice= x.Sum(x=> x.PaidPrice)   
                    }
            );
    }
}


public class ResponseAdjustSearch : IQuery<AResponseAdjust>
{

    
    public IQuery<ExamPartSession> part { get; set; }
    public IQueryable<AResponseAdjust> run(IQueryable<AResponseAdjust> q)
    {
        var oldDb = DependesyContainer.IServiceProvider.GetRequiredService<IAssetManager>();
        return q.Where(x => part.run(oldDb.getDbSet<ExamPartSession>()).Contains(x.response.examPartSession));
    }

    IQueryable<AResponseAdjust> IQuery<AResponseAdjust, AResponseAdjust>.run(IQueryable<AResponseAdjust> q)
    {
        return run(q);
    }

    
}

[Display(Name="جستجوی سریع")]
[ViewAccess(AdminUserRole.SUPPORT,AdminUserRole.SUPER_USER)]
[SelectAccess(AdminUserRole.SUPPORT,AdminUserRole.SUPER_USER)]
public class ResponseAdjustQuickSearch : IQuery<AResponseAdjust>
{

    
    public string phoneNumber { get; set; }
    
    [ForeignKeyAttr(typeof(Models.Exam))]
    public int examId { get; set; }
    public IQueryable<AResponseAdjust> run(IQueryable<AResponseAdjust> q)
    {
        var oldDb = DependesyContainer.IServiceProvider.GetRequiredService<IAssetManager>();
        return q.Where(x => x.response.examPartSession.Customer.phoneNumber.Contains(phoneNumber) && x.response.question.section.ExamId==examId );
    }

    IQueryable<AResponseAdjust> IQuery<AResponseAdjust, AResponseAdjust>.run(IQueryable<AResponseAdjust> q)
    {
        return run(q);
    }

    
}


