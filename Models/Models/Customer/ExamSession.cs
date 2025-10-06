using EnglishToefl.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.TextTools;

namespace Models
{

    public class ExamPartSessionQuery : IQuery<ExamPartSession>
    {
        public IQueryable<ExamPartSession> run(IQueryable<ExamPartSession> q) //where T2 : AsyncableEntiryUser
        {
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var value = serviceCollection.BuildServiceProvider().GetService<Models.User>();
            return q.Where(x => x.CustomerId == value.id);
        }


    }

    [BigTable]
    [ViewAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    [SelectAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    public class ExamSession : AsyncableEntiryUser
    {

        [ForeignKey(nameof(exam))]
        public int examId { get; set; }


        [ForeignKey(nameof(examId))]
        public Exam exam { get; set; }


        public DataSource dataSource { get; set; } = DataSource.FromNewServer;

        public DateTime startTime { get; set; } = DateTime.UtcNow;

        public ExamMode Mode { get; set; }



        [InverseProperty(nameof(Models.ExamPartSession.examSession))]
        public ICollection<ExamPartSession> parts { get; set; }
    }


    public class ResumeExamSession
    {
        public Guid sessionPartId { get; set; }
    }

    [BigTable]
    [FroceFillter<AsyncableEntityUserQuery>(CustomIgnoreTag.Kind.CLIENT)]
    [FroceAction<AsyncableEntityUserAction>(CustomIgnoreTag.Kind.CLIENT)]
    [ViewAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    [SelectAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    public class ExamPartSession : AsyncableEntiryUser
    {
        [NotMapped]
        public List<Models.Section> sessions;


        [ForeignKey(nameof(exam))]
        public int examId { get; set; }


        [ForeignKey(nameof(examId))]
        public Exam exam { get; set; }


        [ForeignKey(nameof(examSession))]
        public Guid? examSessionId { get; set; }

        [ForeignKey(nameof(examSessionId))]
        public ExamSession examSession { get; set; }

        public DateTime startTime { get; set; }



        public EnglishToefl.Models.ExamPartType SectionType { get; set; }



        public ExamMode Mode { get; set; }


        public DataSource dataSource { get; set; } = DataSource.FromNewServer;


        public int? score { get; set; }
        
        [Column(TypeName = "jsonb[]")]
        public List<TextHighLight> highlights { get; set; }
        

        [InverseProperty(nameof(Models.Response.examPartSession))]
        public ICollection<Response> responses { get; set; }




        public bool? isFirst { get; set; } = null;

        //////
        public enum State
        {
            JUST_CREATED = 1,
            ENDED = 2,
            PROCESSED = 3,
            WRONG_LOG = 4,
            Migrated_From_Old_Incurrect = 5
        }


        public State state { get; set; }



        //public string userSeessinId { get; set; }






        //[GridShow]
        //[Column(TypeName = "jsonb[]")]
        //public List<ExamAction> actions { get; set; }






        /*[Column(TypeName = "hstore")]
        public Section[] actions2{get;set;}/**/




        public int dataVersion { get; set; } = 0;


    }

    [BigTable]
    //[FroceFillter<AsyncableEntityUserQuery>(CustomIgnoreTag.Kind.CLIENT)]
    //[FroceAction<AsyncableEntityUserAction>(CustomIgnoreTag.Kind.CLIENT)]
    [SelectAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    [ViewAccess(AdminUserRole.SUPPORT, AdminUserRole.DATA_ENTRY, AdminUserRole.DEVELOPER)]
    public class ExamPartSessionLogs : IIdMapper<Guid>
    {



        [Key]
        [PersianLabel("شناسه")]
        [Models.IgnoreDefultForm]
        public Guid id { get; set; }

        [JsonIgnore]
        [NotMapped]
        public ChangeEventList onChanges { get; set; }

        public object getId()
        {
            return id;
        }

        [Column(TypeName = "jsonb[]")]
        public List<ExamAction> actions { get; set; }



        


        /*[Column(TypeName = "hstore")]
        public Section[] actions2{get;set;}/**/




        public int dataVersion { get; set; } = 0;



        
        public void getQuestionsTime(Dictionary<int,long> qs)
        {

            int lastq = -1;
            long lastqt = 0;
            bool anwserChange = false;
            for (int i=0; i < actions.Count(); ++i)
            {
                if(actions[i] is ChooseOption ch)
                {
                    if (ch.questionId == lastq)
                        anwserChange = true;
                }
                if (actions[i] is GoNext gn)
                {
                    if (qs.ContainsKey(lastq) && anwserChange)
                    {
                        qs[lastq] += gn.time - lastqt;
                        
                    }
                    
                    if (qs.ContainsKey(gn.toPartId))
                    {
                        lastq = gn.toPartId;
                        anwserChange = false;
                        lastqt = gn.time;
                    }
                    if (true)
                    {
                        lastq = gn.toPartId;
                        lastqt = gn.time;
                    }
                        
                }
            }
        }

       
    }



   
  

}
