using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Services;
using TestHelperAI;
using ViewGeneratorBase;

public class StateSaver : IStateSaver
{
    public DbContext context;

    public async Task save(AResponseAdjust speakingAdjustment)
    {
        context.Entry(speakingAdjustment).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
}

public class ReviseRepo
{
    internal readonly DBContext _context;
    internal readonly Guid uId;
    HttpClintContiner cc;


    public ReviseRepo(DBContext context, Guid uId, HttpClintContiner cc = null)
    {
        this._context = context;
        this.uId = uId;
        this.cc = cc;
    }
    private async Task<AWritingAdjust> adjustWritingCreate<T>(WritingResponse response,
        AWritingAdjust rr = null) where T : AWritingAdjust,new()
    {
        if (rr == null)
        {
            if (response.question.section.exam.company.learnBranch == LearnBranch.Toefl)
            {
                rr = new T();
            }
            else
            {
                rr = new IeltsWritingAdjust();
            }

            rr.responseId = response.id;
            rr.title = "Chat GPT-4o";
            rr.createAt = DateTime.UtcNow;
            rr.examinerId = Guid.Parse("5a6d749b-7b1e-40e3-a695-849b21a07914");
            rr.text = response.text;
            rr.state = AdjustingState.Requested;

            _context.responseAdjusts.Add(rr);
            await _context.SaveChangesAsync();
        }

        return rr;
    }

    public async Task reviseWriting0(WritingResponse response, AWritingAdjust rr , DBContext context)
    {
        

        

        try
        {

            var z = new AdjustWritingController();


            var mindMap = response.question.section.keyPoints.FirstOrDefault();
            if (mindMap == null && response.question.section.SectionTypeId == 7)
            {

                mindMap = await z.FindMindMaps(response.question);
                mindMap.sectionId=response.question.SectionId;
                await context.keyPointOfSections.AddAsync(mindMap);

                await context.SaveChangesAsync();
                context.Entry(mindMap).State = EntityState.Detached;
                response.question.section.keyPoints = new List<KeyPointOfSections>()
                {
                    mindMap
                };
            }
            
            
            if (response.question.section.SectionTypeId == 16 && mindMap == null )
            {
                if (response.question.Files is { Count: > 0 })
                {
                    // update mindmap for this question
                    mindMap = await z.ExtractKeyPointsFromImage(Path.Combine(JsonBase64File.UploadFolderPath,
                        response.question.Files[0]));

                    mindMap.sectionId = response.question.SectionId;
                    await context.keyPointOfSections.AddAsync(mindMap);
                

                    await context.SaveChangesAsync();
                    context.Entry(mindMap).State = EntityState.Detached;
                    response.question.section.keyPoints = new List<KeyPointOfSections>()
                    {
                        mindMap
                    };
                }
            }


            if (response.question.section.exam.company.learnBranch == LearnBranch.Toefl)
            {
                {
                    if (rr is TOEFLWritingAdjust rr2)
                        rr2 = await z.AdjustWritingAsync2(response, rr2);
                }
                {
                    if (rr is ResponseAdjust rr2)
                        rr2 = await z.AdjustWritingAsyncOld(response, rr2);
                }
            }
            else
            {
                var rr2 = rr as IeltsWritingAdjust;
                rr2 = await z.AdjustIeltsWritingAsync(response, rr2);
            }

            // TODO: add different states for different levels of progress in prompt completion
            // change state if only both prompts have results
            rr.state = (rr.items != null && rr.items.Count >= 2)
                ? AdjustingState.Compelete
                : AdjustingState.Requested;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            rr.state = AdjustingState.Requested;
        }

        
        
    }

    Dictionary<int, Tuple<Section, DateTime>> sections = new();

    async Task<T> getResponse<T>(T res) where T : Response
    {
        var response = await _context.Responses.AsNoTracking().Where(x => x.id == res.id)
            .Include(x => x.question)
            .ThenInclude(x => x.section)
            .FirstOrDefaultAsync() as T;

        Tuple<Section, DateTime> last = null;
        if (!sections.TryGetValue(response.question.SectionId, out last) || last.Item2.AddMinutes(10) < DateTime.UtcNow)
        {
            last = new Tuple<Section, DateTime>(_context.Sections.AsNoTracking()
                    .Where(x => x.id == response.question.SectionId)
                    .Include(x => x.SectionParts)
                    .Include(x => x.keyPoints)
                    .Include(x => x.exam)
                    .ThenInclude(x => x.company)
                    .FirstOrDefault(),
                DateTime.UtcNow);
            sections[response.question.SectionId] = last;
        }

        response.question.section = last.Item1;
        return response;
    }


    private async Task<ASpeakingAdjustment> adjustSpeakingCreate(SpeakingResponse response,
        ASpeakingAdjustment rr = null)
    {
        if (rr == null)
        {
            if (response.question.section.exam.company.learnBranch == LearnBranch.Toefl)
            {
                rr = new SpeakingAdjustment();
            }
            else
            {
                rr = new IeltsSpeakingAdjustment();
            }

            rr.responseId = response.id;
            rr.title = "Chat GPT-4o";
            rr.createAt = DateTime.UtcNow;
            rr.examinerId = Guid.Parse("5a6d749b-7b1e-40e3-a695-849b21a07914");
            //rr.text = response.text;
            rr.state = AdjustingState.Requested;

            _context.responseAdjusts.Add(rr);
            await _context.SaveChangesAsync();
        }

        return rr;
    }

    public async Task adjustSpkeang0(SpeakingResponse response, ASpeakingAdjustment rr , DBContext context)
    {
        try
        {
            

            var z = new AdjustWritingController();

            var ss = new StateSaver()
            {
                context = context
            };
            if (response.question.section.exam.company.learnBranch == LearnBranch.Toefl)
            {
                var rr2 = rr as SpeakingAdjustment;
                rr = rr2 = await z.AdjustSpeaking(response,
                    Path.Combine(JsonBase64File.UserUploadFolderPath, uId.ToString(), response.file), rr2, ss);
            }
            else
            {
                var rr2 = rr as IeltsSpeakingAdjustment;
                rr = rr2 = await z.AdjustSpeakingForIELTS(response,
                    Path.Combine(JsonBase64File.UserUploadFolderPath, uId.ToString(), response.file), rr2, ss);
            }

            rr.state = AdjustingState.Compelete;


            // TODO: add different states for different levels of progress in prompt completion
            // change state if only both prompts have results
            //rr.state = (rr.items != null && rr.items.Count >= 2) ? AdjustingState.Compelete : AdjustingState.Requested;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            rr.state = AdjustingState.Requested;
        }

        
    }

    public async Task<AWritingAdjust> revise<T>(Guid responseId, AWritingAdjust z = null) where T : AWritingAdjust, new()
    {
        var res = await _context.Responses.Where(x => x.id == responseId).FirstOrDefaultAsync() as WritingResponse;
        var examPart = await _context.ExamPartSessions.FindAsync(res.examPartSessionId);
        if (examPart == null || examPart.CustomerId != uId)
        {
            Console.WriteLine(
                $"[ExamingRepositry] get error  {uId} {res.examPartSessionId} {(examPart?.CustomerId ?? Guid.Empty)}");
            throw new Exception("access dinied");
        }

        WritingResponse response = null;
        if (z == null)
        {
            response = await getResponse<WritingResponse>(res);
            z = await adjustWritingCreate<T>(response, null);
            await _context.SaveChangesAsync();
        }

        if (!proccess.ContainsKey(z.id) || proccess[z.id].task.IsFaulted || (proccess[z.id].task.IsCompleted && proccess[z.id].revise.state!=AdjustingState.Compelete))
        {
            response = await getResponse(res);
            _context.Entry(z).State = EntityState.Detached;
            var cc=new Coroutine2(null);
            proccess[z.id] = new ReviseP()
            {
                revise = z,
                task = cc.Run(async () =>
                {
                    IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                    using var scope = services2.CreateScope();
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<DBContext>();
                    await reviseWriting0(response, z,context);
                    
                    context.Attach(z);
                    context.Entry(z).State = EntityState.Modified;

                    await context.SaveChangesAsync();
                    
                    {
                        res.Score = (decimal)z.finalScore;
                        context.Attach(res);
                        context.Entry(res).State = EntityState.Modified;
                        
                        await context.SaveChangesAsync();
                        try
                        {
                             var ll= await context.Responses
                                .Where(x => x.examPartSessionId == examPart.id && x.Score != null)
                                .GroupBy(x => x.QuestionId)
                                .Select(x => x.OrderBy(x => x.EnterDate).First())
                                .ToListAsync();
                            examPart.score = (int)(ll.Sum(x => x.Score)/2+0.5m);
                            context.Attach(examPart);
                            context.Entry(examPart).State = EntityState.Modified;
                            await context.SaveChangesAsync();
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    
                })
            };
        }


        return proccess[z.id].revise as AWritingAdjust;
        
        
    }

    public class ReviseP
    {
        public AResponseAdjust revise;
        public Task task;
    }

    static Dictionary<Guid, ReviseP> proccess = new Dictionary<Guid, ReviseP>();

    public async Task<ASpeakingAdjustment> adviseSpeaking(Guid responseId, ASpeakingAdjustment z = null)
    {
        var res = await _context.Responses.Where(x => x.id == responseId).FirstOrDefaultAsync() as SpeakingResponse;
        var examPart = await _context.ExamPartSessions.FindAsync(res.examPartSessionId);
        if (examPart == null || examPart.CustomerId != uId)
        {
            Console.WriteLine(
                $"[ExamingRepositry] get error  {uId} {res.examPartSessionId} {(examPart?.CustomerId ?? Guid.Empty)}");
            throw new Exception("access dinied");
        }

        SpeakingResponse response = null;
        if (z == null)
        {
            response = await getResponse(res);
            z = await adjustSpeakingCreate(response, z);
            await _context.SaveChangesAsync();
        }
        
        if (!proccess.ContainsKey(z.id) || proccess[z.id].task.IsFaulted || (proccess[z.id].task.IsCompleted && proccess[z.id].revise.state!=AdjustingState.Compelete))
        {
            response = await getResponse(res);
            _context.Entry(z).State = EntityState.Detached;
            var cc=new Coroutine2(null);
            proccess[z.id] = new ReviseP()
            {
                revise = z,
                
                task = cc.Run(async () =>
                {
                    IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                    using var scope = services2.CreateScope();
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<DBContext>();
                    await adjustSpkeang0(response, z,context);
                    context.Attach(z);
                    context.Entry(z).State = EntityState.Modified;

                    
                    res.Score = (decimal)z.finalScore;
                    context.Attach(res);
                    context.Entry(res).State = EntityState.Modified;
                
                    await context.SaveChangesAsync();
                    
                    
                    

                        
                    try
                    {
                        var ll= await context.Responses
                            .Where(x => x.examPartSessionId == examPart.id && x.Score != null)
                            .GroupBy(x => x.QuestionId)
                            .Select(x => x.OrderBy(x => x.EnterDate).First())
                            .ToListAsync();
                        examPart.score = (int)(ll.Sum(x => x.Score)/4+0.5m);
                        context.Attach(examPart);
                        context.Entry(examPart).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    

                })
            };
        }


        return proccess[z.id].revise as ASpeakingAdjustment;
    }

    public async Task< AResponseAdjust> checkState(Guid reviseId)
    {
        if (proccess.TryGetValue(reviseId, out var value))
        {
            if (value.task.IsCompleted)
            {
                if (value.revise == null)
                {
                    proccess[reviseId] = null;
                }
            }
            else
            {
                await Task.Delay(5 * 1000);
                Task.WaitAny(new Task[] { value.task, Task.Delay(5 * 1000) });
                return value.revise;
            }
        }

        return await reRevise(reviseId);
    }
    
    public async Task<AResponseAdjust> reRevise(Guid reviseId)
    {
        var revise = await _context.responseAdjusts.Include(x => x.response).ThenInclude(x => x.examPartSession)
            .Where(x => x.id == reviseId).SingleAsync();
        if(revise==null)
            return null;
        if (revise.response.examPartSession.CustomerId != uId)
        {
            return null;
        }

        if (revise.state == AdjustingState.Compelete)
            return revise;

        if (revise.state != AdjustingState.Compelete && revise.createAt.AddMinutes(5) < DateTime.UtcNow)
        {
            if (revise is SpeakingAdjustment sr)
                return await adviseSpeaking(revise.responseId, sr);

            {
                if (revise is TOEFLWritingAdjust wr)
                    return await this.revise<TOEFLWritingAdjust>(revise.responseId, wr);
            }

            {
                if (revise is ResponseAdjust wr)
                    return await this.revise<ResponseAdjust>(revise.responseId, wr);
            }
            {
                if (revise is IeltsWritingAdjust wr)
                    return await this.revise<IeltsWritingAdjust>(revise.responseId, wr);
            }

        }

        return revise;
    }
}