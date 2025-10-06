using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Models.TextTools;

public interface IHCC
{
    public HttpClient client { get; set; }
}
public class HttpClintContiner : IHCC
{
    public HttpClient client { get; set; }

}
public class ExamingRepositry
{
    internal readonly DBContext _context;
    internal readonly Guid uId;



    public ExamingRepositry(DBContext context, Guid uId)
    {
        this._context = context;
        this.uId = uId;
    }

    public async Task<int> removeResponse(Guid resId)
    {
        return await _context.Responses.Where(x => x.examPartSession.CustomerId == uId).Where(x => x.id == resId)
            .ExecuteUpdateAsync(x=> x.SetProperty(x=> x.IsRemoved,true)
            ); 
    }
    public async Task<TextHighLight> saveTextHighLight(Guid sesId,TextHighLight vmData)
    {
        var exampart = await _context.ExamPartSessions.FindAsync(sesId);
        if (exampart == null || exampart.CustomerId != uId)
        {
            Console.WriteLine($"[ExaminingRepository] saveTextHighLight get error  {uId} {sesId} {(exampart?.CustomerId ?? Guid.Empty)}");
            throw new Exception("access denied");
        }

        exampart.highlights ??= new List<TextHighLight>();
        
        var idx=exampart.highlights.FindIndex(x => x.id == vmData.id);
        if (idx != -1)
            exampart.highlights[idx] = vmData;
        else
            exampart.highlights.Add(vmData);
        
        _context.Entry(exampart).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return vmData;
    }
    public async Task<int> removeTextHighLight(Guid sesId,string id)
    {
        var exampart = await _context.ExamPartSessions.FindAsync(sesId);
        if (exampart == null || exampart.CustomerId != uId)
        {
            Console.WriteLine($"[ExaminingRepository] saveTextHighLight get error  {uId} {sesId} {(exampart?.CustomerId ?? Guid.Empty)}");
            throw new Exception("access denied");
        }


        if (exampart.highlights==null)
            return 0;
        
        var idx=exampart.highlights.FindIndex(x => x.id == id);
        if (idx == -1)
            return 0;
        exampart.highlights.RemoveAt(idx);
        _context.Entry(exampart).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return 1;
    }
    public async Task<Response> saveResponse(Response data)
    {

        var exampart = await _context.ExamPartSessions.FindAsync(data.examPartSessionId);
        if (exampart == null || exampart.CustomerId != uId)
        {
            Console.WriteLine($"[ExamingRepositry] get error  {uId} {data.examPartSessionId} {(exampart?.CustomerId ?? Guid.Empty)}");
            throw new Exception("access dinied");
        }

        data.question = await _context.Questions.Where(x => x.id == data.QuestionId).Include(x => x.section).ThenInclude(x => x.SectionType).FirstAsync();
        try
        {
            data.clacl();
            data.refresh();
        }
        catch
        {
            Console.WriteLine($"[ExamingRepositry] saveResponse get error {data.QuestionId}");
        }

        if (exampart.SectionType != data.question.section.ExamPartType)
            Console.WriteLine($"[ExamingRepositry] section type not equal for {data.QuestionId}");
        Response res = null;
        if (data.id != null && data.id != Guid.Empty)
        {
            res = await _context.Responses.Where(x => x.id == data.id).FirstOrDefaultAsync();
        }
        else if (data.cid != null && data.cid != Guid.Empty)
        {
            res = await _context.Responses.Where(x => x.cid == data.cid).FirstOrDefaultAsync();
        }

        if (res != null && res.examPartSessionId == data.examPartSessionId) //TODO ef res.examPartSessionId!=data.examPartSessionId should return Custom Error to client  error raise olready but not custom
        {
            res.SyncTime = DateTime.UtcNow;
            res.updatedAt = DateTime.UtcNow;
            _context.Entry(res).CurrentValues.SetValues(data);
            _context.Entry(res).State = EntityState.Modified;
        }
        else
        {
            res = data;
            res.SyncTime = DateTime.UtcNow;
            res.updatedAt = DateTime.UtcNow;

            await _context.Responses.AddAsync(res);
        }

        //await _context.SaveChangesAsync();

        //var examPartUserDataRep=new ExamPartUserDataRepositry(_context, uId);
        //await examPartUserDataRep.addResponseWOS(res, data.question.section);

        await _context.SaveChangesAsync();
        try
        {
            var userRamCacheDataRepository = new UserRamCacheDataRepository(_context);
            (await userRamCacheDataRepository.getOrCreateData(uId)).updateLastRepose(res.updatedAt);

        }
        catch
        {

        }


        var t = await _context.ExamPartSessions.Where(x => x.id == data.examPartSessionId).
            ExecuteUpdateAsync(x =>
                    x.SetProperty(
                        x => x.score,
                        x => _context.Responses.Where(x => x.examPartSessionId == data.examPartSessionId).GroupBy(x => x.QuestionId).Select(x => x.OrderByDescending(x => x.EnterDate).First()).Sum(x => x.Score)
                    )
                );



        return data;
    }

    
}