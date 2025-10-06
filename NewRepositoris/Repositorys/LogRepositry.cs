using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Migrations;
using System.Collections.Generic;
using Models.AiResponse;
using ClientMsgs;

public class NRepositry<T>
{
    internal readonly DBContext _context;


    public NRepositry(DBContext context)
    {
        this._context = context;
   }

}
public class NUserRepositry<T>:NRepositry<T>{
    internal readonly Guid uId;
    public NUserRepositry(DBContext context, Guid uId):base(context)
    {
        this.uId = uId;
    }
}

public class LogRepositry:NUserRepositry<UserLog>
{
    



    public LogRepositry(DBContext context, Guid uId):base(context, uId)
    {
        
    }

    
    
    public async Task<UserLog> saveLog(UserLog data)
    {
        var log=await _context.loggs.Where(x => x.id == data.id).FirstOrDefaultAsync();
        if (log == null)
        {
            data.CustomerId = uId;
            log = data;
            _context.loggs.Add(data);
        }
        else
        {
            _context.Entry(log).CurrentValues.SetValues(data);
            _context.Entry(log).State=EntityState.Modified;
            log = data;
        }
        await _context.SaveChangesAsync();
        return log;
    }
    public async Task<List<TimeSpaningRow>> GetSum(DateTime startTime, DateTime endTime, LearnBranch learnBRanch)
    {
        var z=await _context.loggs.Where(x=> x.CustomerId == uId)
            .Where(x => x.endDate>startTime && x.startDate<endTime)
            .Where(x=> x.learnBranch==learnBRanch)
            .GroupBy(x=> x.startDate.Date)
            .Select(
                x=>new TimeSpaningRow(){
                    key=x.Key,
                    reading=x.Where(x=> x.state==UserLog.State.READING).Sum(y=> (y.endDate-y.startDate).TotalMilliseconds),
                    Listening=x.Where(x=> x.state==UserLog.State.LISTENING).Sum(y=> (y.endDate-y.startDate).TotalMilliseconds),
                    speaking=x.Where(x=> x.state==UserLog.State.SPEAKING).Sum(y=> (y.endDate-y.startDate).TotalMilliseconds),
                    writing=x.Where(x=> x.state==UserLog.State.WRITING).Sum(y=> (y.endDate-y.startDate).TotalMilliseconds),
                    leitner=x.Where(x=> x.state==UserLog.State.LEITNER).Sum(y=> (y.endDate-y.startDate).TotalMilliseconds)
                }

            ).ToListAsync(); /**/
        return z;
    }
    
    public async Task<List<TimeSpaningRow0>> GetSum2(DateTime startTime, DateTime endTime, LearnBranch leanrBranch)
    {
        var z=await _context.loggs
            .Where(x=> x.learnBranch==leanrBranch)
            .Where(x=> x.CustomerId == uId)
            .Where(x => x.endDate>startTime && x.startDate<endTime)
            .GroupBy(x=> x.startDate.Date)
            .Select(
                x=>new TimeSpaningRow0(){
                    key=x.Key,
                    all=x.Sum(y=> (y.endDate-y.startDate).TotalMilliseconds)
                }

            ).ToListAsync(); /**/
        return z;
    }
    public async Task<double> GetSumAll(DateTime startTime, DateTime endTime, LearnBranch leanrBranch)
    {
        var z=await _context.loggs
            .Where(x=> x.learnBranch==leanrBranch)
            .Where(x=> x.CustomerId == uId)
            .Where(x => x.endDate>startTime && x.startDate<endTime)
            .SumAsync(x=> (x.endDate-x.startDate).TotalMilliseconds);
        return z;
    }

}
