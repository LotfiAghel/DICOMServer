using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.EntityFrameworkCore;
using ClientMsgs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using EnglishToefl.Models;

public class UserRamCacheDataRepository : NRepositry<UserSectionData>
{
    public UserRamCacheDataRepository(DBContext context) : base(context)
    {
    }
    public static ConcurrentDictionary<Guid, UserRamCacheData> data = new ConcurrentDictionary<Guid, UserRamCacheData>();

    

    public async Task<UserRamCacheData> getOrCreateData(Guid uId)
    {
        UserRamCacheData tmp;
        data.TryGetValue(uId, out tmp);

        if (tmp == null)
        {
            tmp = await _context.UserRamCacheDatas.Where(x => x.id == uId).FirstOrDefaultAsync();
            if (tmp != null)
            {
                tmp.lastResponse = await _context.Responses.Where(x => x.examPartSession.CustomerId == uId).MaxAsync(x => (DateTime?)x.updatedAt)??DateTime.MinValue;
            }
        }

        if (tmp == null)
        {
            tmp = new()
            {
                id = uId,
                lastResponse = await _context.Responses.Where(x => x.examPartSession.CustomerId == uId).MaxAsync(x => x.updatedAt)
            };
            _context.UserRamCacheDatas.Add(tmp);
            await _context.SaveChangesAsync();
        }
        if (tmp.algVersion < UserRamCacheData.AlgVersion)
        {
            tmp.algVersion = UserRamCacheData.AlgVersion;
            tmp.lastResponseCalculation = DateTime.MinValue; //has bug in cluster with difrent algVersions
        }
        if (tmp.subjectAlgVersion < UserRamCacheData.SubjectAlgVersion)
        {
            tmp.subjectAlgVersion = UserRamCacheData.SubjectAlgVersion;
            tmp.lastSubjectCalculationTime = DateTime.MinValue; //has bug in cluster with difrent algVersions
        }
        data.TryAdd(uId, tmp);
        return  tmp;

    }

    static ConcurrentDictionary<Guid, UserMigrateState> usersc2 = new ConcurrentDictionary<Guid, UserMigrateState>();
    static ConcurrentDictionary<Guid, UserMigrateState> usersc23 = new ConcurrentDictionary<Guid, UserMigrateState>();

    public async Task<string> calcSubjectData(DBContext context, Guid uId)
    {
        if (usersc23.TryGetValue(uId, out var x) && x.endDate > DateTime.UtcNow)
            return $" after {x.endDate} > {DateTime.UtcNow} ";
        usersc23[uId] = new UserMigrateState()
        {
            startDate=DateTime.UtcNow,
            endDate = DateTime.UtcNow.AddSeconds(100)
        };
        var t = await getOrCreateData(uId);
        if (t.lastSectionChnageTime <= t.lastSubjectCalculationTime)
        {
            usersc23[uId].endDate = DateTime.UtcNow;
            return $"nothing to process since {t.lastSectionChnageTime }";
        }


        {
            var subP = new UserSubjectDataRepositry(context, uId);

            var q = context.userSectionDatas
            .Where(x => x.CustomerId == uId)
            .Where(x => x.updatedAt>=t.lastSubjectCalculationTime).Include(x=> x.section);
            

            var userSectionDatas = await q.ToListAsync();
            Dictionary<int, UserSubjectData> mark = new ();
            foreach (var subjectId in userSectionDatas)
            {
                if(subjectId.section.subjectId==null)
                    continue;
                if(!mark.TryGetValue(subjectId.section.subjectId.Value, out var tmp))
                    mark[subjectId.section.subjectId.Value]= tmp = await subP.create(subjectId.section.subjectId.Value);
                else
                {
                    continue;
                }
                
                //foreach (var tm in userSectionData)
                {
                    var z = await tmp.calcSafe(q);
                    if (tmp.mergWith(z))
                        context.UserSubjectDatas.Entry(tmp).State = EntityState.Modified;
                }
                
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            if (t.lastSubjectCalculationTime != t.lastSectionChnageTime)
            {
                t.lastSubjectCalculationTime = t.lastSectionChnageTime;
                await update(t);
            }
        }
        return "--";
    }
        
    public async Task<string> createExamProgressBar(DBContext context, Guid uId)
    {
        string res = "";
        UserMigrateState x = null;
        if (usersc2.TryGetValue(uId, out x) && x.endDate > DateTime.UtcNow)
            return $" after {x.endDate} > {DateTime.UtcNow} ";
        usersc2[uId] = new UserMigrateState()
        {
            startDate=DateTime.UtcNow,
            endDate = DateTime.UtcNow.AddSeconds(1000)
        };
        try
        {
            var t = await getOrCreateData(uId);
            if (t.lastResponse <= t.lastResponseCalculation)
            {
                usersc2[uId].endDate = DateTime.UtcNow;
                return $"nothing to process since {t.lastResponseCalculation }";
            }

            var responsesG = await context.Responses.Where(x => x.examPartSession.CustomerId == uId && x.updatedAt >= t.lastResponseCalculation)
                .Include(x=> x.examPartSession)
                .Include(x => x.question)
                .ThenInclude(x => x.section)
                .ThenInclude(x => x.exam)
                .GroupBy(x => x.question.SectionId).ToListAsync();

            var userSectionDataRepositry = new UserSectionDataRepositry(context, uId);
            var examP2 = new UserExamPartDataRepositry(context, uId);
            var subP = new UserSubjectDataRepositry(context, uId);
            var lastResponse = await context.Responses.Where(x => x.examPartSession.CustomerId == uId && x.updatedAt > t.lastResponseCalculation).OrderBy(x => x.SyncTime).LastOrDefaultAsync(); ;
            if (lastResponse == null)
            {
                usersc2[uId].endDate = DateTime.UtcNow;
                return "nothing to process 2 " ;
            }
            /*await context.ExamPartSessions.Where(x => x.CustomerId == uId && x.isFirst == null).ExecuteUpdateAsync(x=>
                x.SetProperty(
                    x=>x.isFirst,
                    examP => context.ExamPartSessions.Where(x => x.CustomerId == uId && x.examId == examP.examId && x.SectionType == examP.SectionType && x.startTime < examP.startTime).Any()
                )
            );*/

           foreach (var responses in responsesG)
                {
                    var userSectionData = await userSectionDataRepositry.addResponseWOS(responses.Key);
                    await context.SaveChangesAsync();
                

                if (userSectionData.updateWithSessionWOS(responses))
                {
                    userSectionData.updatedAt = DateTime.UtcNow;
                    if (userSectionData.id!=default(Guid))
                        context.Entry(userSectionData).State = EntityState.Modified;
                    
                    await context.SaveChangesAsync();
                }
                
            }
            var sectionsId = responsesG.ConvertAll(x => x.Key);
            {
                var userSectionDatas = await context.userSectionDatas.Where(x => x.CustomerId == uId)
                    .Where(x => sectionsId.Contains(x.sectionId))
                    .GroupBy(x => new { x.section.ExamId, x.section.ExamPartType }).ToListAsync();

                foreach (var userSectionData in userSectionDatas)
                {
                    var tmp = await examP2.create(userSectionData.Key.ExamId, userSectionData.Key.ExamPartType);
                    var z = await tmp.calcSafe(context.userSectionDatas);
                    if (tmp.mergWith(z))
                        context.userExamPartDatas.Entry(tmp).State = EntityState.Modified;
                }
            }
            
            if (t.lastResponseCalculation != t.lastResponse)
            {
                t.lastResponseCalculation = t.lastResponse;
                t.lastSectionChnageTime=t.lastResponseCalculation;
                await update(t);
               
               
            }
            var numberOfChange = await context.SaveChangesAsync();
            res = $"proccess and change {numberOfChange} ";
        }
        catch(Exception ex) 
        {
            Console.WriteLine("progress compute error ");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        usersc2[uId].endDate = DateTime.UtcNow;
        //usersc2.try(uId);
        
        return res;

    }


    public async Task<string> createExamProgressBar2(DBContext context, Guid uId)
    {
        UserMigrateState x = null;
        if (usersc2.TryGetValue(uId, out x) && x.endDate > DateTime.UtcNow)
            return $" after {x.endDate} > {DateTime.UtcNow} ";
        usersc2[uId] = new UserMigrateState()
        {
            startDate = DateTime.UtcNow,
            endDate = DateTime.UtcNow.AddSeconds(1000)
        };
        try
        {
            var t = await getOrCreateData(uId);
            if (t.lastResponse < t.lastResponseCalculation)
            {
                usersc2[uId].endDate = DateTime.UtcNow;
                return $"nothing to process since {t.lastResponseCalculation}";
            }

            var responsesG = await context.Responses.Where(x => x.examPartSession.CustomerId == uId && x.updatedAt >= t.lastResponseCalculation)
                .Include(x => x.examPartSession)
                .Include(x => x.question)
                .ThenInclude(x => x.section)
                .ThenInclude(x => x.exam)
                .GroupBy(x => x.question.SectionId).ToListAsync();

            var userSectionDataRepositry = new UserSectionDataRepositry(context, uId);
            var examP2 = new UserExamPartDataRepositry(context, uId);
            var subP = new UserSubjectDataRepositry(context, uId);
            var lastResponse = await context.Responses.Where(x => x.examPartSession.CustomerId == uId && x.updatedAt >= t.lastResponseCalculation).OrderBy(x => x.SyncTime).LastOrDefaultAsync(); ;
            if (lastResponse == null)
            {
                usersc2[uId].endDate = DateTime.UtcNow;
                return "nothing to process 2 ";
            }
            await context.ExamPartSessions.Where(x => x.CustomerId == uId && x.isFirst == null).ExecuteUpdateAsync(x =>
                x.SetProperty(
                    x => x.isFirst,
                    examP => context.ExamPartSessions.Where(x => x.CustomerId == uId && x.examId == examP.examId && x.SectionType == examP.SectionType && x.startTime < examP.startTime).Any()
                )
            );

            foreach (var responses in responsesG)
            {
                var userSectionData = await userSectionDataRepositry.addResponseWOS(responses.Key);
                await context.SaveChangesAsync();


                if (userSectionData.updateWithSessionWOS(responses))
                {
                    if (userSectionData.id != default(Guid))
                        context.Entry(userSectionData).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }

            }
            var sectionsId = responsesG.ConvertAll(x => x.Key);
            {
                var userSectionDatas = await context.userSectionDatas.Where(x => x.CustomerId == uId)
                    .Where(x => sectionsId.Contains(x.sectionId))
                    .GroupBy(x => new { x.section.ExamId, x.section.ExamPartType }).ToListAsync();

                foreach (var userSectionData in userSectionDatas)
                {
                    var tmp = await examP2.create(userSectionData.Key.ExamId, userSectionData.Key.ExamPartType);
                    var z = await tmp.calcSafe(context.userSectionDatas);
                    if (tmp.mergWith(z))
                        context.userExamPartDatas.Entry(tmp).State = EntityState.Modified;
                }
            }
           
            if (t.lastResponseCalculation != t.lastResponse)
            {
                t.lastResponseCalculation = t.lastResponse;

                await update(t);


            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("progress compute error ");
            Console.WriteLine(ex.StackTrace);
        }
        usersc2[uId].endDate = DateTime.UtcNow;
        //usersc2.try(uId);
        var numberOfChange = await context.SaveChangesAsync();
        return $"proccess and change {numberOfChange} ";

    }


    public async Task update(UserRamCacheData urmcData)
    {
        _context.UserRamCacheDatas.Entry(urmcData).State=EntityState.Modified;
        await _context.UserRamCacheDatas.Where(x => x.id == urmcData.id).ExecuteUpdateAsync(
            x => x.SetProperty(
                x => x.lastResponseCalculation,
                urmcData.lastResponseCalculation

                ).SetProperty(
                x => x.lastResponse,
                urmcData.lastResponse

                ).SetProperty(
                    x => x.lastSectionChnageTime,
                    urmcData.lastSectionChnageTime

                ).SetProperty(
                    x => x.lastSectionCalculationTime,
                    urmcData.lastSectionCalculationTime

                ).SetProperty(
                    x => x.lastSubjectCalculationTime,
                    urmcData.lastSubjectCalculationTime
                ).SetProperty(
                    x => x.subjectAlgVersion,
                    urmcData.subjectAlgVersion
                )

            );
        
    }
}
