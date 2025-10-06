using System;
using System.Collections.Generic;
using EnglishToefl.Models;
using Microsoft.AspNetCore.Http;
using Models;
namespace ClientMsgs
{
    public class ObjectContainerRespone<T> : BooleanResponse
    {
        public T data { get; set; }
        public List<ExtraMsgItem> items { get; set; }
        public ObjectContainerRespone() { }
        public ObjectContainerRespone(T data)
        {
            this.data = data;
        }
    }
    
    public class LongProccessRespone<T> : ObjectContainerRespone<T>
    {
        public int secondForTry { get; set; } = 0; 
        public LongProccessRespone() { }
        public LongProccessRespone(T data)
        {
            this.data = data;
        }
    }

    public class ReviseSmallData
    {
        public Guid id{ get; set; }
        public DateTime createAt{ get; set; }
        public AdjustingState state{ get; set; }
        public Guid responseId { get; set; }
    }
    public class AdviseLimit
    {
        //public int totalPayeDailyLimit;

        public class Row
        {
            public DateTime validUntil { get; set; }
            public int total { get; set; }
        }
        public int totalDaily { get; set; }
        //public List<Row> limits { get; set; }
        public int inDayUsed { get; set; }
        
        public int extraWithToken { get; set; }
        public List<ReviseSmallData> inDayUsedList { get; set; }


        //public int neoRemain { get; set; }
        //public int neoTotal { get; set; }
        //public int payeRemain { get; set; }
        //public int payeTotal { get; set; }
        
    }
    public class UserMigrateState
    {
        public int exams { get; set; } = 0;
        public int totalExams { get; set; } = -1;
        public bool done { get; set; } = false;
        public DateTime endDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public DateTime startDate { get; set; } = DateTime.UtcNow;
        public int leitnerTotal { get; set; } = -1;
        public int leitnerNumber { get; set; } = 0;
    }
    public class ProfileEdit
    {
        public string userName { get; set; }
        public string name { get; set; }
        public DateTime? birthDay { get; set; }
        public int? weight { get; set; }
        public int? height { get; set; }
        public Gender? gender { get; set; }

        public string disease { get; set; }
        public string avatar { get; set; }
        public string bio { get; set; }

    }
    public class ProfileEditResponse : BooleanResponse
    {
        public Profile profile { get; set; }
    }
    public class ExamPart
    {
        public ForeignKey2<Models.Exam, int> examId;
        public ExamPartType sectionType;
        public int resCount;

        public ExamPart(ForeignKey2<Models.Exam, int> foreignKey2, ExamPartType sectionType, int v = 0)
        {
            this.examId = foreignKey2;
            this.sectionType = sectionType;
            this.resCount = v;
        }

       
    }
    public class AvatarUploadResult : BooleanResponse
    {
        public string StoredFileName { get; set; }
        public Profile profile {get;set;}

    }
    public class UploadAvatar
    {



        


        public IFormFile File { get; set; }

        

    }
}