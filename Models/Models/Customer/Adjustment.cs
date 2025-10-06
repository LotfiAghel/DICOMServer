using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Models
{

    public class KeyPointTitle
    {
        public string title { get; set; }
        public List<string> keyPoints { get; set; }
    }

    [Display(Name = "نکات اصلی سکشن")]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    [SelectAccess(AdminUserRole.DATA_ENTRY)]
    [FroceFillter<ResponseAdjustFilter>(CustomIgnoreTag.Kind.CLIENT)]
    public class KeyPointOfSections : AIdMapper<Guid>
    {
        [ForeignKey(nameof(section))]
        public int sectionId { get; set; }

        public Section section { get; set; }


        [Column(TypeName = "jsonb[]")]
        public List<KeyPointTitle> titles { get; set; }
        
        [Column(TypeName = "jsonb[]")]
        public List<KeyPointTitle> imageDescriptions { get; set; }
    }
    
    public class ResponseFilterWithCompany:IQuery<Response>{
        
        [MultiSelect]
        public List<ForeignKey2<Company,int>> companis { get; set; }
        public IQueryable<Response> run(IQueryable<Response> q)
        {
            return q.Where(x => companis.Select(x => x.Value).Contains(x.examPartSession.exam.CompanyId));
        }
    }
    



   



    public class ResponseAdjustFilter : IQuery<AResponseAdjust>
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResponseAdjustFilter(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public IQueryable<AResponseAdjust> run(IQueryable<AResponseAdjust> q) //where T2 : AsyncableEntiryUser
        {

            var userId = _httpContextAccessor.HttpContext.getUser2Id();
            return q.Where(x => x.response.examPartSession.CustomerId == userId || x.examinerId==userId);

        }
    }

    public enum AdviseMethod
    {
        CHAT_GPT_4O=1,
    }

    public class AdviseRequest
    {
        public ForeignKey2<Response,Guid> responseId { get; set; }
        
        public AdviseMethod method { get; set; }
        public bool iWantNow { get; set; }
    }
    public class AllOfExamReviseRequest
    {
        [ForeignKeyAttr(typeof(ExamSession))]
        public Guid examId { get; set; }
        
        public AdviseMethod method { get; set; }
        public bool iWantNow { get; set; }
    }
    
    

    public class ResponseAdjustsearch : IQuery<ResponseAdjust>
    {
        public AdjustingState ?state { get; set; }
        public DateTime ?from { get; set; }
        public DateTime ?end { get; set; }
        public IQueryable<ResponseAdjust> run(IQueryable<ResponseAdjust> q)
        {
            if(state!=null)
                q=q.Where(x => x.state == state.Value);
            if(from!=null)
                q=q.Where(x => x.createAt >from);
            if(end!=null)
                q=q.Where(x => x.createAt <end);
            return q;
        }
    }


    public class ResponseAdjustItem
    {

    }

    public class PronunciationReport 
    { 
        public double? score { get; set; }
        public string text { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        
        [CollectionAttr(typeof(FileString))]
        public List<string> files { get; set; }
        public override string ToString()
        {
            return $"{text} - {start}:{end}";
        }
    }

    public class PronunciationReportPart : ResponseAdjustItem
    {
        public List<PronunciationReport> PronunciationReports { get; set; }
    }

    public struct CorrectingPart
    {
        public string before { get; set; }
        public Range<int> range { get; set; }
        public string after { get; set; }
        public string reason { get; set; }
        
        [CollectionAttr(typeof(FileString))]
        public List<string> files { get; set; }
        public enum CorrectingType
        {
            Unknown = 0,
            Typo = 1,
            Grammer = 2,
        }
        public CorrectingType correctingType { get; set; }
    }
    public class CorectingAdjustItem : ResponseAdjustItem
    {
        public string title { get; set; }
        public string before { get; set; }
        public string after { get; set; }

        public string partColor { get; set; } = "grean";
        
        [CollectionAttr(typeof(FileString))]
        public List<string> files { get; set; }

        public List<CorrectingPart> items { get; set; }

    }
    
    public enum AdjustingState
    {
        None = 0,
        Requested = 1,
        Adjusting = 2,
        Compelete = 3
    }
    public class AdjustSort : IQuery<AResponseAdjust>
    {
        public IQueryable<AResponseAdjust> run(IQueryable<AResponseAdjust> q)
        {
            return q.OrderBy(x => x.createAt);
        }
    }
    

    [BigTable]
    [Display(Name = "تصحیح")]
    [InsertAccess(AdminUserRole.SUPPORT)]
    [UpdateAccess(AdminUserRole.SUPPORT)]
    [SelectAccess(AdminUserRole.SUPPORT)]
    [FroceFillter<ResponseAdjustFilter>(CustomIgnoreTag.Kind.CLIENT)]
    [DefultSort<AdjustSort>]
    public abstract class AResponseAdjust : IIdMapper<Guid>
    {
        [Key]
        [PersianLabel("شناسه")]
        [Models.IgnoreDefultForm]
        public Guid id { get; set; }

        public Guid responseId { get; set; }
        [ForeignKey(nameof(responseId))]
        public Response response { get; set; }


        [ForeignKey(nameof(examiner))]
        public Guid? examinerId { get; set; }

        [ForeignKey(nameof(examinerId))]
        public User examiner { get; set; }


        public string title { get; set; }

        public AdjustingState state { get; set; }

        public DateTime createAt { get; set; }

        public string AdjustmentStatus { get; set; }
        public string text { get; set; }


        [Column(TypeName = "jsonb[]")]
        public List<ResponseAdjustItem> items { get; set; }
        
        
        public float? finalScore { get; set; }
        
        public float? CoherenceScore { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [NotMapped]
        public ChangeEventList onChanges { get; set; }


        public object getId()
        {
            return id;
        }
    }


    public abstract class AWritingAdjust:AResponseAdjust
    {
        public float? TaskScore { get; set; }

        public string TaskCompleteness { get; set; }
        
        
        [Column("grammatical")]
        public string Grammatical { get; set; }
        
        [Column("grammatical_score")]
        public float? GrammaticalScore { get; set; }
    }
    
    [BigTable]
    [Display(Name = "تصحیح رایتینگ")]
    [InsertAccess(AdminUserRole.DATA_ENTRY)]
    [UpdateAccess(AdminUserRole.DATA_ENTRY)]
    [FroceFillter<ResponseAdjustFilter>(CustomIgnoreTag.Kind.CLIENT)]
    [DefultSort<AdjustSort>]
    public class ResponseAdjust : AWritingAdjust
    {

        [Column("organization_score")]
        public float? OrganizationScore { get; set; }
        
        [Column("language_use_score")]
        public float? LanguageUseScore { get; set; }

        [Column("essay_organization")]
        public string? EssayOrganization { get; set; }

    }



    public interface IIeltsAdjust
    {
        [Column("lexical_score")]
        public float? LexicalScore { get; set; }
        
        [Column("lexical")]
        public string Lexical { get; set; }
        
        [Column("coherence")]
        public string Coherence { get; set; }
        
        
        [Column("grammatical")]
        public string Grammatical { get; set; }
        
        [Column("grammatical_score")]
        public float? GrammaticalScore { get; set; }

    }
    public class TOEFLWritingAdjust : AWritingAdjust
    {
        [Column("language_use_score")]
        public float? LanguageUseScore { get; set; }
        public float? DevelopmentScore { get; set; }

        [Column("organization_score")]
        public float? OrganizationScore { get; set; }
        
        [Column("language_use")]
        public string? LanguageUse { get; set; }
        public string? DevelopmentSuggestions { get; set; }
        
        [Column("essay_organization")]
        public string? EssayOrganization { get; set; }
        

    }
    public class IeltsWritingAdjust : AWritingAdjust,IIeltsAdjust
    {

        [Column("lexical")]
        public string Lexical { get; set; }
        
        [Column("lexical_score")]
        public float? LexicalScore { get; set; }
        
        
       


        
        [Column("coherence")]
        public string Coherence { get; set; }
        
        
        



    }


    public class ASpeakingAdjustment : AResponseAdjust
    {
        [Column(TypeName = "jsonb")]
        public Transcript transcript { get; set; }
        public float? FluencyScore { get; set; }
    }
    
    [Display(Name = "تصحیح اسپیکینگ")]
    public class SpeakingAdjustment : ASpeakingAdjustment
    {

        public string Developement { get; set; }
        public string Delivery { get; set; }
        
        [Column("language_use")]
        public string LanguageUse { get; set; }

        public float? DeliveryScore { get; set; }
        public float? DevelopementScore { get; set; }

        public float? LanguageUseScore { get; set; }
        
        
    }

    [Display(Name = "تصحیح اسپیکینگ")]
    public class IeltsSpeakingAdjustment : ASpeakingAdjustment,IIeltsAdjust
    {

        [Column("coherence")]
        public string Coherence { get; set; }
        
        [Column("lexical")]
        public string Lexical { get; set; }
        
        [Column("lexical_score")]
        public float? LexicalScore { get; set; }
        
        [Column("grammatical")]
        public string Grammatical { get; set; }
        
        
        [Column("grammatical_score")]
        public float? GrammaticalScore { get; set; }
        
        
        

        
        public float? PronunciationScore { get; set; }



    }

}
