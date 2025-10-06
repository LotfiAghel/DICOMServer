using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Data.Data;
using Data.Migrate;
using Data.Migrations;
using EnglishToefl.Data;
using EnglishToefl.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Models.AiResponse;
using Models.Exams;
using Models.TextTools;
using Newtonsoft.Json.Linq;
using Repositoris.AdminReps;
using SGS.Core;
using TestHelperAI;
using TestHelperAI.Models;
using Exam = Models.Exam;
using Question = EnglishToefl.Models.Question;
using ServiceBuy = Models.ServiceBuy;

namespace ConsoleAdmin
{




    public class Program
    {
        static async Task Main3(string[] args)
        {
            var a=new AdjustWritingController();
            for (int i = 0; i < 100; ++i)
            {
                var z=await AdjustWritingController.WisperSpeachToText("/media/lotfi/145410dd-e98c-46a9-aece-7d3001835df31/all/1.8T/programing/TestHelper/backup-bade-hach/Upload/0a0d3418-cbdf-4fc1-8740-678fe9281e72^0^318.mp3");
                await Main3(args);
            }
        }
        
        
        public static IHost host;

        public static List<string> customers = null;
         public static async Task migrateServiceBuys2(ApplicationDbContext oldContext,DBContext context)
        {
            
            customers = await oldContext.Customers.Select(x => x.id).ToListAsync();
            //customers = await oldContext.ServiceUseLogs.OrderByDescending(x=> x.id).Select(x => x.CustomerId).Take(10).ToListAsync();
            
            //var zf = async (int of, int mod) =>
            {
                //IServiceProvider services2 = DependesyContainer.serviceCollection.BuildServiceProvider();
                for (int i = 0; i < customers.Count(); i += 1)
                {
                    //if (DateTime.Now.Hour > 7)
                    //    return;
                    var uid = customers[i];
                   // using (var scope = services2.CreateScope())
                    {
                        //var services = scope.ServiceProvider;
                        try
                        {
                            //var context = services.GetRequiredService<DBContext>();
                            //var oldContext = services.GetRequiredService<ApplicationDbContext>();
                            if (i == 0)
                            {
                                //await Migrator.migrateTable<EnglishToefl.Models.ServiceBuy, Models.ServiceBuy, int, int>(oldContext.ServiceBuys, context.ServiceBuys);
                            }
                            Console.WriteLine($"user {uid}");
                            var user = await UserMigrator.MigrateUser(oldContext, context, oldContext.Customers.Find(uid));
                            await ExamMigrator.migrateServiceBuys2(oldContext, context, Guid.Parse(uid));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("--");
                            
                        }
                       
                    }
                    


                }
                
                

            };
            /*int n = 1;
            for (int i = 0; i < n; ++i)
            {
                var c = new Coroutine2(new AsyncContextThread());
                var x = i;
                _ = c.Run(async () => await zf(x, n));
            }*/
            //_ = Coroutine2.Instance2.Run(async () => await zf(1, 2));

            


        }
        
         
         
         public static async Task<int> ExcuteUpdateInApp<T>(DbContext context,IQueryable<T> inp, Func<T,T> action)
         {
            
                 var inp2 = await inp.ToListAsync();
                 foreach (var e in inp2)
                 {
                     try
                     {
                        var e2=action(e);
                        context.Entry(e2).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                     }
                     catch
                     {
                         context.ChangeTracker.Clear();
                         continue;
                     }
                     
                 }

                 return await context.SaveChangesAsync();
             
         }
        public static async Task Main2(string[] args)
        {
            var builder = CreateHostBuilder(args);
            host = builder.Build();

            //Models.ServicesTool.services=host.Services;
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DBContext>();
                    var esr=new ExamStatisticsRepo(context);
                    var usr=await context.users.Where(x => x.id == Guid.Parse("cd1fbbf1-4018-4bd6-96e0-91f80cd84cbf"))
                        .Include(x => x.clients).FirstAsync();
                    await esr.calcFirstExams(usr.id);
                    {
                        var usrs = await context.ExamPartSessions
                            .Where(x => x.exam.CompanyId == 10 && x.startTime > DateTime.Now.AddDays(-50))
                            .Select(x => x.CustomerId).GroupBy(x=> x).ToListAsync();
                        if (false)
                            foreach (var u in usrs)
                            {
                                await esr.calcFirstExams(u.Key);
                            }
                    }
                    var exams=context.Exams.Where(x => x.CompanyId == 10).OrderBy(x=> x.PartOrder).Select(x => x.id).ToList();
                    foreach (var examId in exams)
                    {



                        

                        var tw0 = await context.ExamPartSessions.FindAsync(
                            Guid.Parse("ab27107e-d582-46eb-a18c-76c1ecbf61be"));
                        await esr.getTime(await context.examPartSessionLogs.FindAsync(tw0.id));




                        await esr.calcQuestionsStatistics(examId, true);
                    }

                    return;
                    
                    var bs=await context.ServiceBuys.Where(x => x.BuyTime>DateTime.Now.AddDays(-30)).SumAsync(x=> x.Service.Price);
                    
                    bs=await context.ServiceBuys.Where(x => x.BuyTime>DateTime.Now.AddDays(-30) && x.Service.ServiceGroupId==20).SumAsync(x=> x.Service.Price);
                    
                    var z=await context.users.CountAsync(x => x.examSessions.Any(
                        x => 

                        x.exam.company.learnBranch==LearnBranch.Ielts) );
                    z=await context.users.CountAsync(x => x.examSessions.Any(
                        x => 

                            x.exam.company.learnBranch==LearnBranch.Toefl && x.startTime>DateTime.Now.AddDays(-30)) );
                     z=await context.ExamSessions.CountAsync(x => x.exam.company.learnBranch==LearnBranch.Ielts) ;
                     
                     
                     z=await context.users.CountAsync(x => x.registerDate>DateTime.Now.AddDays(-60) );
                    
                    var q2=await context.Questions.Where(x =>
                        x.GetType() == typeof(QuestionTrueFalseOption)
                        && (x as QuestionTrueFalseOption).questionOptions.Count(x => x.IsCorrect) == 2
                        && x.section.exam.company.learnBranch == LearnBranch.Ielts).ToListAsync();
                    var q26=await context.Questions.Where(x =>
                        x.GetType() == typeof(QuestionTrueFalseOption)
                        && (x as QuestionTrueFalseOption).questionOptions.Count(x => x.IsCorrect) == 2
                        && x.section.exam.company.learnBranch == LearnBranch.Ielts).ExecuteUpdateAsync(x=> x.SetProperty(x=> x.Mark,2));
                    if (false)
                    {
                        var wqs = await context.Questions.Where(x =>
                                x.GetType() == typeof(WordQuestion) &&
                                x.section.exam.company.learnBranch == LearnBranch.Ielts)
                            .ToListAsync();
                        await context.Questions.Where(x =>
                            x.GetType() == typeof(WordQuestion) &&
                            x.section.exam.company.learnBranch == LearnBranch.Ielts).ExecuteUpdateAsync(
                            x => x.SetProperty(
                                x => x.Mark,
                                1
                            )
                        );
                    }
                    ///SUBJECT

                    var ieltsWQ = context.Sections.Where(x =>
                        x.exam.company.learnBranch == LearnBranch.Ielts && x.ExamPartType == ExamPartType.Writing);
                    var ieltsRQ = context.Sections.Where(x =>
                        x.exam.company.learnBranch == LearnBranch.Ielts && x.ExamPartType == ExamPartType.Listening);
                    await ieltsRQ
                        .ExecuteUpdateAsync(
                            x => x.SetProperty(x => x.SectionTypeId, 19)
                        );
                    await ieltsWQ.Where(x=> x.id==x.exam.Sections.Where(x=>x.ExamPartType==ExamPartType.Writing).OrderBy(x=> x.PartOrder).Take(1).Last().id)
                        .ExecuteUpdateAsync(
                            x => x.SetProperty(x => x.SectionTypeId, 16)
                        );
                    await ieltsWQ.Where(x=> x.id==x.exam.Sections.Where(x=>x.ExamPartType==ExamPartType.Writing).OrderBy(x=> x.PartOrder).Take(2).Last().id)
                        .ExecuteUpdateAsync(
                            x => x.SetProperty(x => x.SectionTypeId, 17)
                        );
                    //var resps=await context.Responses.Where(x => x.examPartSession.examId >=14 && x.Content.Contains("dw307")).ToListAsync();
                    //var resps=await context.Questions.Where(x=> x.GetType()==typeof(WordQuestion)&& (x as WordQuestion).answerSheet.Where(x=> x.Contains("(")).Any()).ToListAsync();
                    
                    //var oldContext = services.GetRequiredService<ApplicationDbContext>();
                    ///Migrator.initMigrators();
                    //var ids=await context.ServiceBuys.Select(x => x.id).ToListAsync();
                    //var tt=await oldContext.ServiceBuys.Where(x=> !ids.Contains(x.id)).ToListAsync();
                    //var uc = context.ServiceBuys.Where(x=> x.id<=101907+10).Count();
                    //var ucc=context.users.Count();
                    //var oucc=oldContext.Customers.Count();
                    
                    
                    await context.Subjects.ExecuteUpdateAsync(x=> x.SetProperty(x=> x.IsRemoved,false));
                    var ss2 = await context.Subjects
                        .Where(
                            x => context.Subjects.Where(y=> y.id<x.id && y.Name.ToLower()==x.Name.ToLower()).Any()
                        )
                        .ExecuteUpdateAsync(x=> x.SetProperty(x=> x.IsRemoved,true));


                    
                  
                        
                    var subjectMapping = await context.Subjects
                        .Where(x=> !x.IsRemoved)
                        .ToDictionaryAsync(s => s.Name.ToLower(), s => s.id);

// Step 2: Execute the update
                    await ExcuteUpdateInApp(context,context.SectionSubjects.Where(x =>  x.Subject.IsRemoved 
                            //&& x.section.exam.CompanyId ==10
                            //&& context.Subjects.Any(y => y.Name.ToLower() == x.Subject.Name.ToLower() && y.CompanyId == x.section.exam.CompanyId)
                                                             ).Include(x=> x.Subject).AsNoTracking(),
                       
                            x =>
                            {
                                var x2=context.SectionSubjects.Find(x.id);
                                x2.subjectId = subjectMapping[x.Subject.Name.ToLower()];
                                return x2;
                            });
                    
                    var changed=await context.SectionSubjects.Where(x => x.section.exam.CompanyId != x.Subject.CompanyId
                                                             && context.Subjects.Any(y => y.Name == x.Subject.Name)
                                                             
                    ).ExecuteUpdateAsync(
                        x => x.SetProperty(
                            x => x.subjectId,
                            x => context.Subjects.Where(y => y.Name == x.Subject.Name).OrderBy(x=>x.id).First().id

                        )

                    );/**/
                    
                    //await Migrator.migrateTable<EnglishToefl.Models.ServiceBuy, Models.ServiceBuy, int, int>(oldContext.ServiceBuys.OrderByDescending(x=> x.id).ToList(), context.ServiceBuys);
                    //await migrateServiceBuys2(oldContext,context);
                    //await Migrator.migrateTable<EnglishToefl.Models.ServiceBuy, Models.ServiceBuy, int, int>(oldContext.ServiceBuys, context.ServiceBuys);


                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

        }

        public static async Task checkSubject(DBContext db)
        {
            var userId = Guid.Parse("f006fd52-9abf-4ad8-9c9e-e6aa15442a8f");
            var user= await db.users.FindAsync(userId);
            var sections=await db.Responses.Where(x => x.examPartSession.CustomerId == userId)
                .Where(x => x.question.section.subjectId == 6)
                .Where(x=> x.question.section.exam.CompanyId==10)
                .Where(x=> x.examPartSession.SectionType==ExamPartType.Reading)
                .GroupBy(x => x.question.SectionId)
                .ToListAsync();
            
            
            var sections2=await db.userSectionDatas.Where(x => x.CustomerId == userId)
                .Where(x=> x.section.exam.CompanyId==10)
                .Where(x=> x.section.ExamPartType==ExamPartType.Reading)
                .Where(x => x.section.subjectId == 6)
                
                .ToListAsync();
            Console.WriteLine(sections.Count);//barbar ba 33
            Console.WriteLine(sections2.Count);//barbar ba 33
        }


        public static async Task examsChanger(DBContext context)
        {
            var rep=new ExamStatisticsRepo(context);
            //var exams = context.Exams.Where(x => x.CompanyId == 10 ).OrderBy(x=> x.id).ToList();
                    
            //await changeContent(context, context.Exams.Find(1147));
            //var exams = context.Exams.Where(x => x.CompanyId == 2 || x.CompanyId == 10 ||x.CompanyId == 3 ).OrderBy(x=> x.id).ToList();
            List<int> toefls = [10];//[2, 10];
            var exams = context.Exams
                .Where(x => toefls.Contains(x.CompanyId ))
                
                //.Where(x=> x.id==26)
                //.Where(x => x.Name.Contains("Neo 162") )
                //.Where(x=> x.id==1166)
                .OrderByDescending(x=> x.id).Take(3).ToList();
            foreach (var exam in exams)
                try
                {
                    Console.WriteLine(exam.id);
                    await GrammerTreeAndTRanslateWithAi(context, exam);
                    //await rep.changeContent(exam);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--");
                }

            return ;
        }

        public static async Task Subject(DBContext context)
        {
            var subjectMapping22 = await context.Subjects
                .Where(x=> !x.IsRemoved)
                .Select(s => s.id).ToListAsync();
            var subjectMapping20 = await context.Subjects
                .ToListAsync();
            var tttp=context.Sections.Where(x=> (x.subjectId==null /*|| x.subject.IsRemoved*/) && x.SectionSubjects.Any()).Include(x=> x.SectionSubjects).ToList();
            int io = 0;
            foreach (var sec in tttp)
            {
                try
                {
                    var subjectId = sec.subjectId;
                    if (subjectId == null)
                    {
                        subjectId = sec.SectionSubjects.FirstOrDefault(x => subjectMapping22.Contains(x.subjectId))?.subjectId;
                    }

                    if (subjectId == null)
                    {
                        subjectId =sec.SectionSubjects.Select(x => x.subjectId).First();
                        var subj=subjectMapping20.First(x => x.id == subjectId);
                        subj=subjectMapping20.First(x => x.Name == subj.Name && !x.IsRemoved);
                        subjectId=subj.id;
                    }
                    sec.subjectId=subjectId;
                    context.Entry(sec).State = EntityState.Modified;
                }catch
                {
                    Console.WriteLine("88");
                            
                }
                if(io++ % 10 == 0)
                    await context.SaveChangesAsync();
            }
            await context.SaveChangesAsync();
        }
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            host = builder.Build();

            var ss=" China's Tang dynasty (A.D. 618-906), and ".ToWords(
                
                );

            //Models.ServicesTool.services=host.Services;
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    
                    var context = services.GetRequiredService<DBContext>();
                    // Recalculate Speaking ExamPartSessions scores from latest responses (rounded sum/4)
                    //await FixSpeakingScore(context);
                    //await FixWritingingScore(context);

                    /*var q = context.ExamPartSessions;
                        
                    var z=await q.ExecuteUpdateAsync(
                        x => x.SetProperty(
                            x => x.isFirst,
                            x => null
                        )
                    );*/
                    
                    //await checkIsFirstProiblem(context);

                    await examsChanger(context);
                    //await Subject(context);
                    return;
                    //await checkSubject(context);
                    //return;
                    
                    
                    /*await context.Questions.Where(x =>
                            x.section.exam.CompanyId == 31 && x.section.ExamPartType == ExamPartType.Writing  )
                        .ExecuteUpdateAsync(x => x.SetProperty(x => x.Content, x => x.Content.Replace("\n","<br/>")));*/


                    if(false)
                    await context.Questions.Where(x =>
                            x.section.exam.CompanyId == 31 && x.section.ExamPartType == ExamPartType.Writing  && x.Content.Length<4)
                        .ExecuteUpdateAsync(x => x.SetProperty(x => x.Content, x => x.QuestionText));
                    
                    
                    var rep = new ServiceBuyUserRepository2(context);
                    //14->27
                    //2 ->28
                    //26->41
                    var grpId = 26;
                    var serId = 41;
                    var name = "خسارت زیر ساخت";
                    var users=await context.users.Where(x => x.ServiceBuys.Any(s => s.Service.ServiceGroupId == grpId
                                                                    && s.ValidFrom < new DateTime(2025, 06, 29)
                                                                    && s.ValidUntil > new DateTime(2025, 06, 12))
                                                                    && x.ServiceBuys.All(s => !s.Name.Contains(name) || s.ServiceId!=serId)
                                                                    ).ToListAsync();
                        
                    var service = await rep.GetService(serId);
                    
                    
                    foreach (var usr in users)
                    {


                        
                        

                        var boughtService = await rep.GetAllForUserWithServices(usr.id);
                        var maxValid = DateTime.UtcNow;
                        foreach (var s in boughtService)
                        {
                            if (s.Service.ServiceGroupId == service.ServiceGroupId && s.ValidUntil > maxValid)
                                maxValid = s.ValidUntil;
                        }

                        var serviceBuy = new ServiceBuy(service, maxValid, 0)
                        {
                            CustomerId = usr.id,
                            TrackingNumber = 0,
                            transactionCode = "",
                            moneyGateWay = MoneyGateWay.FREE
                        };


                        serviceBuy.Name = name;
                        await rep.Add(serviceBuy);
                    }
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

        }

        private static async Task checkIsFirstProiblem(DBContext context)
        {
            if (true)
            {
                var uId = Guid.Parse("635a0cf6-ed3d-4ea4-b268-967cf6a9d52b");
                uId = Guid.Parse("b4178555-23d0-41b8-8b5d-7bc6d88c81fa");
                var tt=await context.ExamPartSessions.Where(x => x.CustomerId == uId).ExecuteUpdateAsync(x => x.SetProperty(
                    s => s.isFirst,
                    s=> null
                ));
                var rep = new UserExamPartDataRepositry(context, uId);
                var t = await rep.getExamPartSessions(uId, LearnBranch.Toefl);



                
            }

            //var comps = new[] { 10, 2, 15, 31 };
            var comps = new[] { 3,10};//new[] { 2,10};
            var repo = new ExamStatisticsRepo(context);
            var exmas=context.Exams.Where(x=> comps.Contains( x.CompanyId)).OrderBy(x=> x.PartOrder) .ToList();
            
            foreach (var exam in exmas)
            {

                var xx=await context.examStatistics.FindAsync(exam.id);
                if(xx is { readingScores.Count: > 20 })
                    continue;
                
                var exps = await context.ExamPartSessions.Where(x =>
                    x.examId == exam.id
                ).Select(x=> x.CustomerId).Take(200).ToListAsync();
                if (true)
                {
                    foreach (var e in exps)
                    {

                        var q = context.ExamPartSessions
                            .Where(x => x.exam.company.learnBranch == LearnBranch.Toefl)
                            .Where(x => x.CustomerId == e);
                        var z = 1;
                        //while (z>0)
                        {
                            z = await q.Where(x => x.isFirst == null || x.isFirst.Value == true)
                                .Where(x => x.examId == exam.id)
                                .ExecuteUpdateAsync(x =>
                                    x.SetProperty(
                                        session => session.isFirst,
                                        session => !q.Any(y =>
                                            y.examId == session.examId && y.SectionType == session.SectionType &&
                                            y.startTime < session.startTime)
                                    )
                                );
                            var ted = await q.Where(x => x.isFirst != null && x.isFirst.Value).OrderBy(x => x.startTime)
                                .CountAsync();
                            Console.WriteLine(ted);
                        }
                    }
                }

                await repo.calcQuestionsStatistics(exam.id, true);

            }

           

            
            

            
            
            
            
        }

        private static async Task FixSpeakingScore(DBContext context)
        {
            var x=await context.Responses//.Where(r => r.question.section.ExamPartType == ExamPartType.Listening)
                .Where(x=> x.EnterDate>DateTime.UtcNow.AddDays(-120))
                .Where(x=> x.adjusts.Any())
                .ExecuteUpdateAsync(s => s.SetProperty(
                        r => r.Score,
                        r => (decimal?)context.responseAdjusts
                            .Where(x => x.responseId == r.id)
                            .OrderByDescending(x => x.createAt)
                            .First()
                            .finalScore
                    )
                );
            await context.ExamPartSessions
                .Where(s=> s.startTime>DateTime.UtcNow.AddDays(-120))
                .Where(s => s.SectionType == ExamPartType.Speaking)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    sp => sp.score,
                    sp => (int?)(
                        ((context.Responses
                                .Where(r => r.examPartSessionId == sp.id && r.Score != null)
                                .GroupBy(r => r.QuestionId)
                                .Select(g => g.OrderByDescending(r => r.EnterDate).First().Score)
                                .Sum() ?? 0m) / 4m
                        ) + 0.5m
                    )
                ));
        }

        private static async Task FixWritingingScore(DBContext context)
        {
            if (false)
            {
                var x = await context.Responses //.Where(r => r.question.section.ExamPartType == ExamPartType.Listening)
                    .Where(s=> s.examPartSession.CustomerId<Guid.Parse("6342c937-a116-4abc-b174-c6c72c209298"))
                    //.Where(x => x.EnterDate > DateTime.UtcNow.AddDays(-90))
                    .Where(x => x.adjusts.Any())
                    .ExecuteUpdateAsync(s => s.SetProperty(
                            r => r.Score,
                            r => (decimal?)context.responseAdjusts
                                .Where(x => x.responseId == r.id)
                                .OrderByDescending(x => x.createAt)
                                .First()
                                .finalScore
                        )
                    );
            }

            await context.ExamPartSessions
                //.Where(s=> s.startTime>DateTime.UtcNow.AddDays(-90))
                //.Where(s=> s.startTime<DateTime.UtcNow.AddDays(-30))
                .Where(s=> s.CustomerId==Guid.Parse("6342c937-a116-4abc-b174-c6c72c209298"))
                .Where(s => s.SectionType == ExamPartType.Writing)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    sp => sp.score,
                    sp => (int?)(
                        ((context.Responses
                                .Where(r => r.examPartSessionId == sp.id && r.Score != null)
                                .GroupBy(r => r.QuestionId)
                                .Select(g => g.OrderByDescending(r => r.EnterDate).First().Score)
                                .Sum() ?? 0m) / 2m
                        ) + 0.4m
                    )
                ));
        }


        static string sentense(string paragraph) {
            
            // جدا کردن جملات با استفاده از Regex
            string[] sentences = Regex.Split(paragraph, @"(?<=[\.;!\?])\s+");

            for (int i = 0; i < sentences.Length; i++){
                Console.WriteLine($"جمله {i + 1}: {sentences[i]}");
            }

            return paragraph;
        }
        

                public class SentencesNodeC
                {
                    public string context { get; set; }
                    public SentencesNode sentence { get; set; }
                }
          public static async Task<SentencesNode> translateEvryPartDependsOnContext(SentencesNode txt)
        {
            var folder = ".";
            // Build a more context-aware prompt for translation
       
        Console.WriteLine(JToken.FromObject(new SentencesNodeC(){context= txt.farsi_text,sentence=txt}, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString());
        var promptText =
            "Input JSON:\n" +
            JToken.FromObject(new SentencesNodeC(){context= txt.farsi_text,sentence=txt}, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString();
        var fn = $"{folder}/Pr/{promptText.GetSHA256Checksum()}.text";
        
        if (Directory.Exists($"{folder}/Pr") == false)
            Directory.CreateDirectory($"{folder}/Pr");
        bool readRepeat_needed = !File.Exists(fn);
        for (int tries=0; tries<5;++tries)
        {
            if (tries >= 4)
                throw new Exception("Error extracting IELTS question");
            
            GrammarFixPrompt prompt = new GrammarFixPrompt()
            {
                Model = "gpt-4o",
                Name = "translate",
                Description = "grammer tree translator",
                UserMessages={
                    promptText,
                },
                SystemMessages =
                {
                    "You are a translation assistant (english to farsi).\n"+  
                    "You receive a JSON object representing a grammar tree.\n"+  
                    "\n"+
                    "Rules:\n"+
                    "- The root node (`farsi_text` in the top level) must be translated first, to set the global context.\n"+  
                    "- Each child node must be translated **based on the context of its parent node**, not in isolation.  \n"+
                    "- Preserve the structure of the JSON object exactly as received.  \n"+
                    "- Do not change keys or add/remove fields.  \n"+
                    "- Only replace `farsi_text` values with their translations.\n"  
                    
                    
                },
            };

            try
            {
                SentencesNodeC chatResult = null;
                if (readRepeat_needed || true)
                {
                    chatResult = await prompt.ExecutePrompt2<SentencesNodeC>();
                    await File.WriteAllTextAsync(fn, JToken.FromObject(chatResult).ToString(Newtonsoft.Json.Formatting.Indented));
                    Console.WriteLine(JToken.FromObject(chatResult).ToString((Newtonsoft.Json.Formatting)Formatting.Indented));
                    //if(chatResult.fa==null || chatResult.fa.Count!=txt.Length)
                    //    continue;
                }
                else
                {
                    chatResult = JObject
                        .Parse(await File.ReadAllTextAsync(fn))
                        .ToObject<SentencesNodeC>()!;
                }

                return chatResult.sentence;

                readRepeat_needed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex");
            }
        }
        return null;
    }


    public static async Task<string> translateEvryPartDependsOnContext(string txt,string parag)
        {
            var folder = ".";
            // Build a more context-aware prompt for translation
       
        Console.WriteLine(JToken.FromObject(new {context= parag,sentence=txt}, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString());
        var promptText =
            "Input JSON:\n" +
            JToken.FromObject(new {context= parag,sentence=txt}, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString();
        var fn = $"{folder}/Pr/{promptText.GetSHA256Checksum()}.text";
        
        if (Directory.Exists($"{folder}/Pr") == false)
            Directory.CreateDirectory($"{folder}/Pr");
        bool readRepeat_needed = !File.Exists(fn);
        for (int tries=0; tries<5;++tries)
        {
            if (tries >= 4)
                throw new Exception("Error extracting IELTS question");
            
            GrammarFixPrompt prompt = new GrammarFixPrompt()
            {
                Model = "gpt-4o",
                Name = "translate",
                Description = "English to Farsi sentence translator",
                UserMessages = {
                    promptText,
                },
                SystemMessages =
                {
                    "You are a professional translation assistant. " +
                    "Your task is to translate the given English sentence into natural and fluent Farsi, based on the provided context. " +
                    "Focus on meaning and clarity, not word-for-word translation. " +
                    "\n\nInput format (JSON):\n" +
                    "{\n" +
                    "  \"context\": \"context\"   // The full paragraph for background meaning\n" +
                    "  \"sentence\": \"sentence\" // The specific sentence to translate\n" +
                    "}\n\n" +
                    "Output requirement:\n" +
                    "- Return only the Farsi translation of the sentence as a plain string.\n" +
                    "- Do not include any extra characters, quotes, explanations, or formatting.\n" +
                    "- Ensure the translation is smooth, context-aware, and natural in everyday Farsi."
                },
            };


            try
            {
                string chatResult = null;
                if (readRepeat_needed || true)
                {
                    chatResult = await prompt.ExecutePrompt();
                    await File.WriteAllTextAsync(fn, JToken.FromObject(chatResult).ToString(Newtonsoft.Json.Formatting.Indented));
                    Console.WriteLine(JToken.FromObject(chatResult).ToString((Newtonsoft.Json.Formatting)Formatting.Indented));
                    //if(chatResult.fa==null || chatResult.fa.Count!=txt.Length)
                    //    continue;
                }
                else
                {
                    chatResult = JObject
                        .Parse(await File.ReadAllTextAsync(fn))
                        .ToObject<string>()!;
                }

                return chatResult;

                readRepeat_needed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex");
            }
        }
        return null;
    }

    
    public class ListR
    {
        public List<string> result { get; set; }
    }
    public static async Task<List<string>> translateEvryPartDependsOnContexts(List<string> txt)
        {
            var folder = ".";
            // Build a more context-aware prompt for translation
       
        Console.WriteLine(JToken.FromObject(new {sentences=txt}, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString());
        var promptText =
            "Input JSON:\n" +
            JToken.FromObject(txt, Newtonsoft.Json.JsonSerializer.Create(GrammarFixPrompt.aiSerializerSettings)).ToString();
        var fn = $"{folder}/Pr/{promptText.GetSHA256Checksum()}.text";
        
        if (Directory.Exists($"{folder}/Pr") == false)
            Directory.CreateDirectory($"{folder}/Pr");
        bool readRepeat_needed = !File.Exists(fn);
        for (int tries=0; tries<5;++tries)
        {
            if (tries >= 4)
                throw new Exception("Error extracting IELTS question");
            
            GrammarFixPrompt prompt = new GrammarFixPrompt()
            {
                Model = "gpt-4.1",
                Name = "translate",
                Description = "English to Farsi sentence translator",
                UserMessages = {
                    promptText,
                },
                SystemMessages =
                {
                    "You are a professional translation assistant. " +
                    "Your task is to translate the given English sentences into natural and fluent Farsi. " +
                    "Focus on meaning and clarity, not word-for-word translation.\n\n" +
                    "Output rules:\n" +
                    "1. Return ONLY a valid JSON array of strings.\n" +
                    "2. Do NOT include any keys, objects, extra text, or explanations.\n" +
                    "3. Each element in the array must be the Farsi translation of the corresponding input sentence.\n" +
                    "4. The number of items in the output array MUST match the number of input sentences.\n" +
                    "Example input: [\"Hello\", \"How are you?\"]\n" +
                    "Example output:{\"result\": [\"سلام\", \"حالت چطوره؟\"]}"
                } ,
            };


            try
            {
                List<string> chatResult = null;
                if (readRepeat_needed || true)
                {
                    
                    chatResult = (await prompt.ExecutePrompt2<ListR>()).result;
                    await File.WriteAllTextAsync(fn, JToken.FromObject(chatResult).ToString(Newtonsoft.Json.Formatting.Indented));
                    Console.WriteLine(JToken.FromObject(chatResult).ToString((Newtonsoft.Json.Formatting)Formatting.Indented));
                    //if(chatResult.fa==null || chatResult.fa.Count!=txt.Length)
                    //    continue;
                }
                else
                {
                    chatResult = JObject
                        .Parse(await File.ReadAllTextAsync(fn))
                        .ToObject<List<string>>()!;
                }

                return chatResult;

                readRepeat_needed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex");
            }
        }
        return null;
    }

    
    static async Task<Paragraph> getGrammerTree(string content)
        {
            using (var client = new HttpClient())
            {
                var url = "http://localhost:7070/tree";

                
                var json =JToken.FromObject(new{parag= content, pass= "asrbyntnmasd"}).ToString();
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                // ارسال درخواست POST
                var response = client.PostAsync(url, data).Result;

                // خوندن نتیجه
                string result = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine("Server response:");
                Console.WriteLine(result);
                var paragraph = JToken.Parse(result)["tree"].ToObject<Paragraph>();
                
                
                return paragraph;
            }
        }
        static async Task getGrammerTreeTranslate(ParagraphContainer paragraph,string content)
        {
            
            {
                
                //TODO transalte all of pargraph and evry small peace of SentencesNode.farsi_text with chatgpt
                List<SentencesNode> x = [];
                
                foreach (var sentence in paragraph.sentences)
                {
                    if(sentence.c[0].farsi_text is { Length: > 0 })
                        continue;
                    
                    var newSentecNode = await translateEvryPartDependsOnContext(sentence);
                    
                    mergSentencesNode(sentence,newSentecNode);
                    
                    
                    //sentence.f=await translateEvryPartDependsOnContext(sentence.english_text,content);
                        
                    

                }
                
                
            }
        }

        private static void mergSentencesNode(SentencesNode sentence, SentencesNode newSentecNode)
        {
            if (sentence == null || newSentecNode == null)
                return;

            // Set farsi_text from new node
            sentence.f = newSentecNode.farsi_text;

            // Recursively merge children if both have children
            if (sentence.childern != null && newSentecNode.childern != null)
            {
                int count = Math.Min(sentence.childern.Count, newSentecNode.childern.Count);
                for (int i = 0; i < count; i++)
                {
                    mergSentencesNode(sentence.childern[i], newSentecNode.childern[i]);
                }
            }
        }

        private static async Task GrammerTreeAndTRanslateWithAi(DBContext context, Exam exam)
        {
            context.ChangeTracker.Clear();
            var qqs = await context.SectionParts
                .Where(x => x.Section.ExamId == exam.id && x.Section.ExamPartType == ExamPartType.Reading)
                .ToArrayAsync();

            await context.SectionParts.ExecuteUpdateAsync(x => x.SetProperty(
                s => s.paragraphs,
                s => null
            ));
            foreach (var sectionPart in qqs)
                try
                {
                    
                    if (sectionPart.Content == null || sectionPart.Content.Length < 100)
                        continue;
                    var pars = sectionPart.Content?.Split("\n").Where(s => s.Length >= 2).ToArray();

                    if (sectionPart.paragraphs2 == null || sectionPart.paragraphs2.Count<2)
                    {
                        sectionPart.paragraphs2 = new List<ForeignKey2<ParagraphContainer, Guid>>();
                        var pars22 = new List<Paragraph>();
                        foreach (var variable in pars)
                            pars22.Add(await getGrammerTree(variable));

                        
                        //sectionPart.paragraphs = pars22;
                        foreach (var VARIABLE in pars22)
                        {
                            
                            var t = new ParagraphContainer()
                            {
                                sentences = VARIABLE.sentences
                            };
                            context.ParagraphContainers.Add(t);
                            await context.SaveChangesAsync();
                            sectionPart.paragraphs2.Add(t.id);
                            context.SectionParts.Entry(sectionPart).State = EntityState.Modified;
                        }
                        await context.SaveChangesAsync();

                    }

                    var parsd=await context.ParagraphContainers.Where(x => sectionPart.paragraphs2.Select(x=> x.Value).Contains(x.id)).ToListAsync();
                    for (int i = 0; i < parsd.Count; i++)
                    {
                        if (parsd[i].sentences[0].farsi_text == null)
                        {

                            var tmp = await translateEvryPartDependsOnContexts(parsd[i].sentences
                                .Select(x => x.english_text).ToList());

                            for (int j = 0; j < parsd[i].sentences.Count; j++)
                                parsd[i].sentences[j].f = tmp[j];
                            context.ParagraphContainers.Entry(parsd[i]).State = EntityState.Modified;
                        }
                        await context.SaveChangesAsync();



                        await getGrammerTreeTranslate(parsd[i], pars[i]);
                        context.ParagraphContainers.Entry(parsd[i]).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                        context.ChangeTracker.Clear();
                    }
                    await context.SaveChangesAsync();



                    //context.SectionParts.Entry(sectionPart).State = EntityState.Modified;
                    //await context.SaveChangesAsync();
                    context.ChangeTracker.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ConsoleAdmin.Startup>();
                    //webBuilder.UseStartup<EnglishToefl.Data.Startup>();

                });
    }



}
