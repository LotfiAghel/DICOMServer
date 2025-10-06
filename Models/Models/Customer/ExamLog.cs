using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{


    public  class ExamAction
    {
        public long time { get; set; } //startTime.addMiliSeconds(time)
        public long inExamRealTime { get; set; } //agar pause beshe in adad jelo nemire
        public string ?tabId { get; set; }
    }

    public class ChooseOption : ExamAction
    {
        public ForeignKey2<Question, int> questionId { get; set; }
        public int option { get; set; }
    }

    public abstract class ChangePart : ExamAction
    {
        public int toPartId { get; set; }
    }

    public class GoNext : ChangePart
    {
    }
    public class GoBack : ChangePart
    {
    }
    public class ShowAnswer : ExamAction
    {
        public int questionId { get; set; }
        public long closeTime { get; set; }
    }
    public class ShowAnalys : ExamAction
    {
        public int questionId { get; set; }
        public long closeTime { get; set; }
    }


    public class ResumeFromMenu : ExamAction
    {
        
    }
    public class F5Action : ExamAction
    {

    }
    public class F5ResumeAction : ExamAction
    {

    }

}
