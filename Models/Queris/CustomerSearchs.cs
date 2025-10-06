using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Models;

public class SmsByIp : IQuery<EnglishToefl.Models.SendSmsHistory>
{
    public string ip { get; set; }
        
    public IQueryable<EnglishToefl.Models.SendSmsHistory> run(IQueryable<EnglishToefl.Models.SendSmsHistory> q)
    {
        List<byte> ipAddress = new ();
        var ipp = ip.Split(".");
        foreach (var z in ipp)
            ipAddress.Add((byte)(int.Parse(z)));
        var z2= q.Where(x => ipAddress != null && x.IpAddress == ipAddress.ToArray());
        var ss = z2.ToQueryString();
        return z2;
    }

}
public class SmsByPhoneNumberRange : IQuery<EnglishToefl.Models.SendSmsHistory>
{
        
    public TimeRangeRequest range { get; set; }

    public IQueryable<EnglishToefl.Models.SendSmsHistory> run(IQueryable<EnglishToefl.Models.SendSmsHistory> q)
    {
            
        var z = q.Where(x =>  x.SendTime > range.start && x.SendTime < range.end);
        return z;
    }
}


public class SmsByPhoneNumber : IQuery<EnglishToefl.Models.SendSmsHistory>
{
    public string phoneNumber { get; set; }
        

    public IQueryable<EnglishToefl.Models.SendSmsHistory> run(IQueryable<EnglishToefl.Models.SendSmsHistory> q)
    {
            
        var z = q.Where(x => x.PhoneNumber == phoneNumber );
        return z;
    }
}
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

