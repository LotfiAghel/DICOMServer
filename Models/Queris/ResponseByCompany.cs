using System.Collections.Generic;
using System.Linq;
using EnglishToefl.Models;

namespace Models;



public class ResponseBuyEnterDay:IQuery<Response,IGrouping<int,Response>>
{
    public IQueryable<IGrouping<int,Response>> run(IQueryable<Response> q)
    {
        return q.GroupBy(x => x.EnterDate.Day);
    }
}



public class ResponseUsers:IQuery<Response,User>
{
    public IQueryable<User> run(IQueryable<Response> q)
    {
        return q.Select(x => x.examPartSession.Customer);
    }
}