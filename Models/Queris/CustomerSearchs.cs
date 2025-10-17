using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Models;




public class SmsByPhoneNumber2 : IQuery<VerificationCode>
{
    public string phoneNumber { get; set; }


    public IQueryable<VerificationCode> run(IQueryable<VerificationCode> q)
    {

        return  q.Where(x => x.phoneNumber == phoneNumber);
            
    }
}



public class GraphGroupBy<T> 
{
    public IQuery<T> query;
    public IQuery<T,IGrouping<int, T>> groupBy;


    public IQueryable<IGrouping<int, T>> run(IQueryable<T> q)
    {
        q=query.run(q);
        return groupBy.run(q);
    }
    public IQueryable<int> runNumber(IQueryable<T> q)
    {
        return run(q).Select(x=> x.Count());
    }
}

