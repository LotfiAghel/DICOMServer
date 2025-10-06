using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using EnglishToefl.Data;
using Models;
using EnglishToefl.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ViewGeneratorBase;
using WebApplication.Controllers;
using WebApplication.Services;
using EnglishToefl.Data.Repositories.Users;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ClientMsgs;
using Data.Migrate;
using EnglishToefl.Models;
using Models.AiResponse;
using Models.TextTools;
using SGS.Core;
using Company = Models.Company;
using Exam = Models.Exam;
using ExamMode = Models.ExamMode;
using ExamPartSession = Models.ExamPartSession;
using Response = Models.Response;
using Section = Models.Section;


namespace WebApplication.Controllers
{
    [Route("v1/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ExamingController : UsersBaseController
    {

        internal readonly DBContext _context;
        
        ServiceBuyUserRepository2 serviceBuyRepository;
        
        public ExamingController(SectionUserRepository0 repository, DBContext context,  IUserService userService, ServiceBuyUserRepository2 serviceBuyRepository) : base(userService)
        {
            
            this._context = context;
            this.serviceBuyRepository = serviceBuyRepository;
        
        }
        public class OldRes
        {
            public GetByExamIdAndSectionRes value;
            public int statusCode;
            public string v2;

            public OldRes(GetByExamIdAndSectionRes res)
            {
                this.value = res;
                statusCode = 200;
            }

            public OldRes(int statusCode, string v2)
            {
                this.statusCode = statusCode;
                this.v2 = v2;
            }
        }
        [HttpPost()]
        public async Task<ActionResult<ExamSession>> startExamSession([FromBody] FullExamStartViewModel vm)
        {
            //await ExamMigrator.migrateServiceBuys(_oldContex,_context,this.getUserId());
            var exam = await _context.Exams.FindAsync(vm.ExamId);
            var com = await _context.Companies.FindAsync(exam.CompanyId);

            var company = await _context.Companies.FindAsync(exam.CompanyId);
            var userId = this.getUserId();
            var user = await this.getNUser();
            
            if(!user.phoneNumber.StartsWith("fza"))
                if (com.serviceType!=ServiceType.None || exam.serviceType!=ServiceType.None)
                {
                    var access = await serviceBuyRepository.CheckAndUseExamAccess(userId, exam,com);
                    if (!access)
                    {
                        return this.StatusCode(225, "" + exam.CompanyId);
                    }
                }


            var ex = new ExamSession()
            {

                examId = vm.ExamId,
                CustomerId = this.getUserId(),
            };
            _context.ExamSessions.Add(ex);
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();
            return ex;
        }

        public class ExamPartSession2 : ExamPartSession
        {
            public Company Company { get; internal set; }
            public Exam Exam { get; internal set; }
        }
        [HttpPost()]
        public async Task<ActionResult<ExamPartSession2>> startExamPartSession([FromBody] ExamIdAndSectionPartTypeViewModel vm)
        {
            //await ExamMigrator.migrateServiceBuys(_oldContex,_context,this.getUserId());
            //Console.WriteLine(
            /*var oldres=await oldGetByExamIdAndSectionType(vm);
            if (oldres == null)
                return BadRequest();
            if(oldres.statusCode!=200)
                return this.StatusCode(225, oldres.v2);*/
            //var oldsec=await _oldContex.ExamPartSessions.FindAsync(oldres.value.examPartSessionId);
            var exam = await _context.Exams.FindAsync(vm.ExamId);
            var com = await _context.Companies.FindAsync(exam.CompanyId);
            exam.company = await _context.Companies.FindAsync(exam.CompanyId);
            var user = await this.getNUser();
            if(!user.phoneNumber.StartsWith("fza"))
                if (com.serviceType!=ServiceType.None || exam.serviceType!=ServiceType.None)
                {
                    var access = await serviceBuyRepository.CheckAndUseExamAccess(this.getUserId(), exam,com);
                    if (!access)
                    {
                        return this.StatusCode(225, "" + exam.CompanyId);
                    }
                }
            if (vm.examSessionId == null)
            {
                var ex = new ExamSession()
                {

                    examId = vm.ExamId,
                    CustomerId = this.getUserId(),
                };
                _context.ExamSessions.Add(ex);
                await _context.SaveChangesAsync();
                vm.examSessionId = ex.id;
            }
            var res = new ExamPartSession()
            {
                //id= oldsec.id,
                examId = vm.ExamId,
                CustomerId = getUserId(),
                startTime = DateTime.UtcNow, //oldsec.StartTime,
                SectionType = vm.ExamPartType,// oldsec.SectionType,
                Mode = (ExamMode)(int)vm.Mode,//(ExamMode)(int)(oldsec.Mode),
                examSessionId = vm.examSessionId,
                //sessions= oldres.value.sections,
                dataSource = DataSource.FromNewServer,

            };
            await _context.ExamPartSessions.AddAsync(res);
            await _context.SaveChangesAsync();
            var sections = await _context.Sections.Where(x => x.ExamId == vm.ExamId && x.ExamPartType == vm.ExamPartType).OrderBy(x => x.PartOrder).ToListAsync();
            if (exam.CompanyId == 10)
                try
                {
                    var lec2 = sections.Where(x => x.Title == "Conversation 2").FirstOrDefault();
                    if (lec2 == null)
                        lec2 = sections[3];
                    if (lec2 != null)
                        lec2.TimeToAnswer = 65 * 6;
                }
                catch
                {

                }

            var res2 = new ExamPartSession2()
            {
                id = res.id,
                examId = vm.ExamId,
                CustomerId = getUserId(),
                startTime = res.startTime,
                SectionType = res.SectionType,
                Mode = (ExamMode)(int)(res.Mode),
                examSessionId = res.examSessionId,
                sessions = sections,
                Exam = exam,
                Company = exam.company
            };

            return res2;

        }



        [HttpPost()]
        public async Task<ActionResult<ExamPartSession>> resumeExamPartSession([FromBody] ResumeExamSession vm)
        {
            //await ExamMigrator.migrateServiceBuys(_oldContex,_context,this.getUserId());
            //Console.WriteLine(
            /*var oldres=await oldGetByExamIdAndSectionType(vm);
            if (oldres == null)
                return BadRequest();
            if(oldres.statusCode!=200)
                return this.StatusCode(225, oldres.v2);*/
            //var oldsec=await _oldContex.ExamPartSessions.FindAsync(oldres.value.examPartSessionId);
            var res = await _context.ExamPartSessions.FindAsync(vm.sessionPartId);
            if(res.CustomerId!=getUserId())
                return StatusCode(404);
            var exam = await _context.Exams.FindAsync(res.examId);
            exam.company = await _context.Companies.FindAsync(exam.CompanyId);
            var user = await this.getNUser();
            if(!user.phoneNumber.StartsWith("fza"))
                if (exam.CompanyId <13  && exam.CompanyId != 4)
                {
                    var access = await serviceBuyRepository.CheckAndUseExamAccess(this.getUserId(), exam,exam.company);
                    if (!access)
                    {
                        return this.StatusCode(225, "" + exam.CompanyId);
                    }
                }







            return res;

        }

        


        public async Task<ExamPartSession> saveExamPartSession(ExamPartSession exampart)
        {
            //vm.exam
            var uId = this.getUserId();
            exampart.CustomerId = uId;
            var ex = await _context.ExamPartSessions.FirstOrDefaultAsync(x => x.cid == exampart.cid);
            if (ex != null)
                return ex;
            await _context.ExamPartSessions.AddAsync(exampart);
            await _context.SaveChangesAsync();
            return exampart;
        }
        public async Task<ExamSession> saveExamSession(ExamSession exampart)
        {
            //vm.exam
            var uId = this.getUserId();
            exampart.CustomerId = uId;
            var ex = await _context.ExamSessions.FirstOrDefaultAsync(x => x.cid == exampart.cid);
            if (ex != null)
                return ex;
            await _context.ExamSessions.AddAsync(exampart);
            await _context.SaveChangesAsync();
            return exampart;
        }

        public async Task<ObjectContainer<Response>> saveResponse([FromBody] ObjectContainer<Response> vm)
        {
            //vm.exam
            var uId = this.getUserId();
            var rep = new ExamingRepositry(_context, uId);
            var tt = new ObjectContainer<Response>(await rep.saveResponse(vm.data));
            Console.WriteLine(tt.data.id);
            return tt;
        }
        
        
        [HttpPost("{examPartId}")]
        public async Task<ObjectContainer<TextHighLight>> saveHighLight([FromRoute]Guid examPartId,[FromBody] ObjectContainer<TextHighLight> vm)
        {
            //vm.exam
            var uId = this.getUserId();
            var rep = new ExamingRepositry(_context, uId);
            var tt = new ObjectContainer<TextHighLight>(await rep.saveTextHighLight(examPartId,vm.data));
            Console.WriteLine(tt.data.id);
            return tt;
        }
        
        [HttpDelete("{examPartId}")]
        public async Task<ObjectContainer<int>> removeTextHighLight([FromRoute]Guid examPartId,[FromBody] ObjectContainer<string> idc)
        {
            //vm.exam
            var uId = this.getUserId();
            var rep = new ExamingRepositry(_context, uId);
            var tt = new ObjectContainer<int>(await rep.removeTextHighLight(examPartId,idc.data));
            return tt;
        }
        
        [HttpDelete("{resId}")]
        public async Task<int> removeResponse([FromRoute] Guid resId)
        {
            //vm.exam
            var uId = this.getUserId();
            var rep = new ExamingRepositry(_context, uId);
            var tt = await rep.removeResponse(resId);
            return tt;
        }

        [HttpPut]
        public async Task<ObjectContainer<Response>> UploadVoice([FromForm] UploadResponeFile viewModel)
        {
            var uId = this.getUserId();
            var res = await _context.Responses.FindAsync(viewModel.responseId) as SpeakingResponse;
            var exampart = await _context.ExamPartSessions.FindAsync(res.examPartSessionId);
            if (exampart.CustomerId != uId)
                return null;
            var path = Path.Combine(JsonBase64File.UserUploadFolderPath, uId.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var name = res.id;



            if (viewModel.File?.Length > 0)
            {
                //large file
                if (viewModel.File?.Length > 200 * 1024 * 1024)
                {
                    throw new Exception("Big file for voice upload");
                }
                var fn = $"{path}/{name}.mp3";
                using (var stream = new FileStream(fn, FileMode.Create))
                {
                    await viewModel.File.CopyToAsync(stream);
                }
                res.file = $"{name}.mp3";
                _context.Entry(res).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return new ObjectContainer<Response>(res);
        }

        public class TimeOfQ
        {
            public class QuestionTimeRow
            {
                public int qId { get; set; }
                public int time { get; set; }
            }
            public List<QuestionTimeRow> questionTimes { get; set; }
                
        }
        
        [HttpGet("{examPartId}")]
        public async Task<TimeOfQ> getTimes([FromRoute] Guid examPartId)
        {
            var qs = await _context.Responses.Where(x => x.examPartSessionId == examPartId).Select(x => x.QuestionId)
                .ToListAsync();
            var qs2 =new  Dictionary<int, long>();
            foreach (var v in qs)
            {
                qs2[v] = 0;
            }
            (await _context.examPartSessionLogs.FindAsync(examPartId))?.getQuestionsTime(qs2);

            return new TimeOfQ()
            {
                questionTimes = qs2.Select(x => new TimeOfQ.QuestionTimeRow() { qId = x.Key, time = (int)x.Value }).ToList()
            };
        }
        
        [HttpGet]
        public async Task<ObjectContainerRespone<Content>> analyse([FromRoute] int questionId)
        {
            return new ObjectContainerRespone<Content>(await _context.aiResponseContents.Where(x=> x.QuestionId == questionId && !x.IsRemoved).OrderByDescending(x=> x.createdAt).FirstOrDefaultAsync());
        }

        [HttpGet]
        public async Task<AdviseLimit> adviseLimits()
        {
            var uId = this.getUserId();
            var buys = await _context.ServiceBuys.AsNoTracking()
                       .Where(x => x.CustomerId == uId  &&  x.BuyTime < DateTime.Now && DateTime.Now < x.ValidUntil  && (x.NumberOfLeitnerAdd > 1000 || x.Service.NumberOfDolphin > 10)).
                       OrderBy(x => x.ValidUntil).ToListAsync();
            
            var lastDay = DateTime.Now.endOfLastComplteDay();;
            var inDayUsed = await _context.responseAdjusts.Where(x => x.response.examPartSession.CustomerId == uId && x.createAt > lastDay).CountAsync();
            return new AdviseLimit()
            {
                inDayUsed= inDayUsed,
                totalDaily = buys.Count*2,

                /*payeTotal = 4 - inDayUsed,
                neoTotal = 4-inDayUsed,
                
                payeRemain = 4 - inDayUsed,
                neoRemain = 4 - inDayUsed,
                limits =buys.Select(x=> new AdviseLimit.Row(){ validUntil=x.ValidUntil, total=(int)((x.ValidUntil-x.BuyTime).TotalDays)*2}).ToList()*/
            };

        }

        
        /**
         * Return the difficulty level of Section 2 based on
         * how many questions were correct in Section 1.
         */
        static GreSectionDifficulty GetSection2LevelVerbal(int section1Correct)
        {
            if (section1Correct <= 4)
            {
                return GreSectionDifficulty.EASY;
            }
            else if (section1Correct <= 8)
            {
                return GreSectionDifficulty.MEDIUM;
            }
            else
            {
                return GreSectionDifficulty.HARD;
            }
        }
        
        
         private static readonly GreSectionDifficulty[] EasyFirst =
                { GreSectionDifficulty.EASY, GreSectionDifficulty.MEDIUM, GreSectionDifficulty.HARD };
        
            private static readonly GreSectionDifficulty[] MediumFirst =
                { GreSectionDifficulty.MEDIUM, GreSectionDifficulty.HARD, GreSectionDifficulty.EASY };
        
            private static readonly GreSectionDifficulty[] HardFirst =
                { GreSectionDifficulty.HARD, GreSectionDifficulty.MEDIUM, GreSectionDifficulty.EASY };

     public async Task<List<Section>> GetSection2(IQueryable<Section> q, decimal section1Correct)
     {
         // ترتیب سختی‌ها بر اساس تعداد جواب درست
          GreSectionDifficulty[] priorities =
                     section1Correct <= 4 ? EasyFirst :
                     section1Correct <= 8 ? MediumFirst : HardFirst;
     
         foreach (var difficulty in priorities)
         {
             var res = await q.Where(x => x.greSectionDifficulty == difficulty).ToListAsync();
             if (res.Count > 0)
                 return res;
         }
     
         return new List<Section>(); // در صورتی که هیچ چیزی پیدا نشد
     }

        [HttpGet("{examSessionId}")]
        public async Task<List<Section>> GetSection2LevelVerbal([FromRoute] Guid examSessionId)
        {
            decimal ?section1Correct = 0;
            //_context.ExamPartSessions.Where(x=> x.examSessionId==examSessionId && x.SectionType==ExamPartType.Reading )
            try
            {
                
                section1Correct=_context.Responses.Where(x =>
                            _context.ExamPartSessions.Where(x=> x.examSessionId==examSessionId && x.SectionType==GreType.Verbal).Contains(x.examPartSession)
                            
                            &&
                    x.question.section.greSectionDifficulty == GreSectionDifficulty.NONE)
                    .GroupBy(x => x.QuestionId).Select(x => x.OrderByDescending(x => x.EnterDate).First()).ToList().Sum(x => x.Score);
                    

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            if (section1Correct == null)
                section1Correct = 0;

            var examSession=await _context.ExamSessions.FindAsync(examSessionId);
            var q = _context.Sections
                .Where(x => x.ExamId == examSession.examId && x.ExamPartType == ExamPartType.Reading); 
            

            return await GetSection2(q,section1Correct.Value);

        }
        
        [HttpGet("{examSessionId}")]
        public async Task<List<Section>> GetSection2LevelQuant([FromRoute] Guid examSessionId)
        {
            decimal ?section1Correct = 0;
            //_context.ExamPartSessions.Where(x=> x.examSessionId==examSessionId && x.SectionType==ExamPartType.Reading )
            try
            {
                
                  
                section1Correct=_context.Responses.Where(x =>
                        _context.ExamPartSessions.Where(x=> x.examSessionId==examSessionId && x.SectionType==GreType.Quant).Contains(x.examPartSession)
                            
                        &&
                        x.question.section.greSectionDifficulty == GreSectionDifficulty.NONE)
                    .GroupBy(x => x.QuestionId).Select(x => x.OrderByDescending(x => x.EnterDate).First()).ToList().Sum(x => x.Score);



            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            if (section1Correct == null)
                section1Correct = 0;

            var examSession=await _context.ExamSessions.FindAsync(examSessionId);
            var q = _context.Sections
                .Where(x => x.ExamId == examSession.examId && x.ExamPartType == GreType.Quant);
            return await GetSection2(q,section1Correct.Value);

        }


        
        
        // A 2D array where:
//   - Each row corresponds to # of correct answers in Section 1 (0–12)
//   - Each column corresponds to # of correct answers in Section 2 (0–15)
//   - The cell value is the final scaled Verbal score.
        static int[,] verbalScoreTable = new int[,]{
            // Row 0 (Section 1 correct = 0)
            {130, 130, 130, 130, 131, 131, 132, 132, 132, 133, 133, 133, 134, 134, 150, 151},
            // Row 1 (Section 1 correct = 1)
            {130, 130, 130, 131, 131, 132, 132, 132, 133, 133, 133, 134, 134, 135, 152, 153},
            // Row 2
            { 130, 130, 131, 131, 132, 132, 132, 133, 133, 133, 134, 134, 135, 135, 152, 153 },
            // Row 3
            { 130, 131, 131, 132, 132, 132, 133, 133, 133, 134, 134, 135, 135, 135, 152, 153 },
            // Row 4
            { 131, 131, 132, 132, 132, 133, 133, 133, 134, 134, 135, 135, 135, 136, 152, 153 },

            // Row 5
            { 144, 145, 145, 146, 146, 146, 147, 147, 147, 147, 148, 148, 148, 148, 154, 155 },
            // Row 6
            { 144, 145, 146, 146, 146, 147, 147, 147, 147, 148, 148, 148, 148, 149, 154, 155 },
            // Row 7
            { 144, 145, 146, 146, 147, 147, 147, 147, 148, 148, 148, 148, 149, 149, 154, 155 },
            // Row 8
            { 145, 145, 146, 147, 147, 147, 147, 148, 148, 148, 148, 149, 149, 149, 154, 155 },

            // Row 9
            { 149, 150, 151, 151, 151, 151, 152, 152, 152, 152, 153, 153, 153, 153, 159, 160 },
            // Row 10
            { 149, 150, 151, 151, 151, 152, 152, 152, 152, 153, 153, 153, 153, 154, 159, 160 },
            // Row 11
            { 149, 150, 151, 151, 152, 152, 152, 152, 153, 153, 153, 153, 154, 154, 159, 160 },
            // Row 12
            { 149, 150, 151, 152, 152, 152, 152, 152, 153, 153, 153, 154, 154, 154, 159, 160 },
        };
        
        
    }
}