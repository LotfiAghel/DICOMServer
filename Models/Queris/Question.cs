using EnglishToefl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Queris
{

    [PersianLabel("با شناسه")]
    public class QSearchById : SearchById<Question, int>
    {


    }
    [PersianLabel("با محتوا")]
    public class QSearchByContetnt : IQuery<Question>
    {
        [Display(Name = "صورت سوال")]
        [DataType(DataType.MultilineText)]
        public string questionText { get; set; }
        public string content { get; set; }
        public string option { get; set; }
        
        
        public string answerSheet { get; set; }
        
        //public ForeignKey2<Section,int> ?sectionId { get; set; }
        
        [Display(Name = "نام آزمون")]
        public string examName { get; set; }

        public IQueryable<Question> run(IQueryable<Question> q)
        {
            if(!string.IsNullOrEmpty(content))
                q= q.Where(x => x.Content.Contains(content));
            if(!string.IsNullOrEmpty(questionText))
                q= q.Where(x => x.QuestionText.Contains(questionText));
            if(!string.IsNullOrEmpty(option))
                q= q.Where(x => x.GetType().IsInstanceOfType(typeof(QuestionOptinal)) && (x as QuestionOptinal).questionOptions.Any(x=> x.Content.Contains(option)));

            
            if(!string.IsNullOrEmpty(answerSheet))
                q= q.Where(x => x.GetType().IsInstanceOfType(typeof(WordQuestion)) && (x as WordQuestion).answerSheet.Any(x=> x.Contains(option)));

            if (!string.IsNullOrEmpty(examName))
            {
                var parts=examName.ToLower().Split(" ");
                foreach(var i in parts)
                    q=q.Where(x => x.section.exam.Name.ToLower().Contains(i));
                return q;
            }
            
            return q;
        }
    }
    public class QuestionNotHaveContent : IQuery<Question>
    {
        [MultiSelect]
        public List<ForeignKey2<Company,int>> inComponay { get; set; }
        
        [MultiSelect]
        public List<ForeignKey2<Exam, int>> inExams { get; set; }
        
        [MultiSelect]
        public List<ExamPartType> parts { get; set; }
        public IQueryable<Question> run(IQueryable<Question> q)
        {
            q= q.Where(x => 
                !x.contents.Where(x=> !x.IsRemoved).Any() 
                && (x is QuestionTrueFalseOption || x is WordQuestion) 
                
              );
            if (inExams != null)
                q=q.Where(x => inExams.Select(x => x.Value).Contains(x.section.ExamId));
            if (inComponay != null)
                q = q.Where(x=>inComponay.Select(x => x.Value).Contains(x.section.exam.CompanyId));
            if (parts != null)
                q = q.Where(x=>parts.Contains(x.section.SectionType.ExamPartType));
            if (parts != null)
                q = q.Where(x => parts.Contains(x.section.ExamPartType));
            return q;
        }
    }
}
