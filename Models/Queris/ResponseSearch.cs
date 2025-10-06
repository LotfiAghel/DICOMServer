using EnglishToefl.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    [PersianLabel("با مهارت")]
    public class ExamPartSessionSkillSearch : IQuery<ExamPartSession>
    {

        public ExamPartType type { get; set; }
        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.SectionType == type);
        }
    }
    [PersianLabel("با آخرین تغیر در جواب")]
    public class ExamPartSessionChangeTimeSearch : IQuery<ExamPartSession>
    {

        public Range<DateTime> range { get; set; }
        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.responses.Max(x => x.SyncTime) > range.start && x.responses.Min(x => x.SyncTime) < range.start);
        }
    }

    [PersianLabel("ترکیبی")]
    public class ExamPartSessionListSearch : ListQuery<ExamPartSession>
    {

    }

    [PersianLabel("با شناسه")]
    public class ExamPartSessionSearchById : SearchById<ExamPartSession, Guid>
    {

    }
    [PersianLabel("با شناسه")]
    public class ResponseSearchById : SearchById<Response, Guid>
    {

    }
    [PersianLabel("با شناسه")]
    public class OldResponseSearchById : SearchById<EnglishToefl.Models.Response, Guid>
    {

    }

 

    [PersianLabel("ترکیبی")]
    public class ResponseListSearch : ListQuery<Response>
    {

    }

   
    [PersianLabel("با شماره و آزمون")]
    public class ExamPartSessionSearch : IQuery<ExamPartSession>
    {
        //public ForeignKey2<User,Guid> userId { get; set; }
        public string phoneNumber { get; set; }
        public ForeignKey2<Exam, int> examId { get; set; }


        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.Customer.phoneNumber == phoneNumber && x.examId == examId.Value);
        }
    }
    [PersianLabel("با کاربر")]
    public class ExamPartSessionSearchUser : IQuery<ExamPartSession>
    {
        
        public ForeignKey2<User, Guid> userId { get; set; }


        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.CustomerId == userId);
        }
    }
    [PersianLabel("با آزمون")]
    public class ExamPartSessionSearchExam : IQuery<ExamPartSession>
    {
        public ForeignKey2<Exam, int> examId { get; set; }


        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.examId == examId.Value);
        }
    }
    [PersianLabel("با تاریخ")]
    public class ExamPartSessionTimeSearch : IQuery<ExamPartSession>
    {

        public Range<DateTime> range { get; set; }
        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            return q.Where(x => x.startTime >= range.start && x.startTime <= range.end);
        }
    }



    [PersianLabel(" پاسخها")]
    public class ResponseOfExamParts : IQueryConvertor<ExamPartSession,Response>
    {

        
        public IQueryable<Response> run(IQueryable<ExamPartSession> q)
        {
            return null;
        }
    }
    
    [PersianLabel("با جلسه آزمون")]
    public class ResponseExamPartSearch : IQuery<Response>
    {
        public IQuery<ExamPartSession> part { get; set; }
        public IQueryable<Response> run(IQueryable<Response> q)
        {
            var oldDb = DependesyContainer.IServiceProvider.GetRequiredService<IAssetManager>();
            return q.Where(x => part.run(oldDb.getDbSet<ExamPartSession>()).Contains(x.examPartSession));
        }
    }


    [PersianLabel("با سوال ")]
    public class ResponseByQuestionSearch : IQuery<Response>
    {
        public ForeignKey2<Question,int> question { get; set; }
        public IQueryable<Response> run(IQueryable<Response> q)
        {
            return q.Where(x => x.QuestionId==question.Value);
        }
    }

    public class ExamPartSessionFilterWithCompany:IQuery<ExamPartSession>{
        
        [MultiSelect]
        public List<ForeignKey2<Company,int>> companis { get; set; }
        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q)
        {
            var comps = companis.Select(x => x.Value);
            return q.Where(x => comps.Contains(x.exam.CompanyId));
        }
    }


}
