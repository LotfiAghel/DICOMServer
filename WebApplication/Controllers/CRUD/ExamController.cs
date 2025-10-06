using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.Target;

namespace WebApplication.Controllers
{
    [Authorize]
    [Route("v1/generic/Models__Exam")]
    public class ExamController : GenericClientCrudController<Models.Exam, int, DBContext>
    {
        public ExamController(DBContext context, IUserService userService) : base(context, userService)
        {
        }
        [HttpGet("{id}/questionsOfType/{type}")]
        public async Task<List<Question>> questionsOfType(int id, EnglishToefl.Models.ExamPartType type)
        {
            return  _context.Set<Question>().Where(x => x.section.ExamId == id  && x.section.ExamPartType == type)
                .OrderBy(x => x.section.PartOrder)
                .ThenBy(x => x.section.id)
                .ThenBy(x => x.PartOrder).ThenBy(x=> x.id).ToList();
        }
        
        
        [HttpGet("{id}/questionsOfTypeStatistics/{type}")]
        public async Task<List<QuestionStatistics>> questionsOfTypeStatistics(int id, EnglishToefl.Models.ExamPartType type)
        {
            return  _context.Set<Question>().Where(x => x.section.ExamId == id  && x.section.ExamPartType == type)
                .Join(_context.Set<QuestionStatistics>(),
                    question => question.id,
                    stats => stats.id,
                    (question, stats) => new { question, stats })
                .OrderBy(x => x.question.section.PartOrder)
                .ThenBy(x => x.question.section.id)
                .ThenBy(x => x.question.PartOrder)
                .ThenBy(x=> x.question.id)
                .Select(x=>x.stats).ToList();
        }
        
        [HttpGet("{id}/numberOfQuestions")]
        public async Task<int> numberOfQuestions(int id)
        {
           
            return _context.Set<Question>().Where(x => x.section.ExamId == id).Count();
        }
    }


    
   /* [Authorize]
    [Route("v1/generic/Models__Section")]
    public class SectionController : GenericClientCrudController<Models.Section, int, DBContext>
    {
        public SectionController(DBContext context, IUserService userService) : base(context, userService)
        {
        }
        [HttpGet("{id}/questionsOfType/{type}")]
        public async Task<List<Question>> questionsOfType(int id, EnglishToefl.Models.ExamPartType type)
        {
            return  _context.Set<Question>().Where(x => x.section.ExamId == id  && x.section.ExamPartType == type)
                .OrderBy(x => x.section.PartOrder)
                .ThenBy(x => x.section.id)
                .ThenBy(x => x.PartOrder).ThenBy(x=> x.id).ToList();
        }
        [HttpGet("{id}/numberOfQuestions")]
        public async Task<int> numberOfQuestions(int id)
        {
           
            return _context.Set<Question>().Where(x => x.section.ExamId == id).Count();
        }
    }*/

    
    [Authorize]
    [Route("v1/generic/Models__Target__StudentTask")]
    public class StudentTaskController(DBContext context, IUserService userService)
        : SavableRemovableController<StudentTask, Guid>(context, userService)
    {
      
    }
    
    [Authorize]
    [Route("v1/generic/Models__Target__RealScoreTarget")]
    public class RealScoreTargetTargetController(DBContext context, IUserService userService)
        : SavableRemovableController<RealScoreTarget, Guid>(context, userService)
    {
      
    }
    
    [Authorize]
    [Route("v1/generic/Models__Target__MockScore")]
    public class MockScoreTargetTargetController(DBContext context, IUserService userService)
        : SavableRemovableController<MockScore, Guid>(context, userService)
    {
      
    }



    public class GetByQ
    {
        public string modelName { get; set; }
        public string modelId { get; set; }
        
    } 
    
    [Authorize]
    [Route("v1/generic/Models__TextTools__UserNote")]
    public class UserNoteController(DBContext context, IUserService userService)
        : SavableRemovableController<Models.TextTools.UserNote, Guid>(context, userService)
    {
        
        [HttpPost("getByQ")]
        public async Task<List<Models.TextTools.UserNote>> getByQ([FromBody] GetByQ q)
        {
            var res=await _db.Where(x=> x.CustomerId==getUser2Id()).Where(x=> x.modelName==q.modelName && x.modelId == q.modelId).ToListAsync();
            
            return res;
        }

        [HttpGet("getBy/{modelName}/{modelId}")]

        public async Task<List<Models.TextTools.UserNote>> getByQ2([FromRoute] string modelName, [FromRoute] string modelId)
        {
            var res=await _db.Where(x=> x.CustomerId==getUser2Id()).Where(x=> x.modelName==modelName && x.modelId == modelId).ToListAsync();
            return res;
        }
        
        
        [HttpPost("updateTags/{id}")]
        public async Task<Models.TextTools.UserNote> updateCardTags([FromRoute] Guid id,[FromBody] List<ForeignKey2<Models.TextTools.UserNoteTag, Guid>> tags)
        {
            
            var res=await _db.Where(x=> x.CustomerId==getUser2Id() && x.id==id).FirstOrDefaultAsync();
            if (res == null)
                return null;
            res.tags = tags;
            _db.Update(res);
            await _context.SaveChangesAsync();
            return res;
        }
    }
    [Authorize]
    [Route("v1/generic/Models__TextTools__UserNoteTag")]
    public class UserNoteTagController(DBContext context, IUserService userService)
        : SavableRemovableController<Models.TextTools.UserNoteTag, Guid>(context, userService)
    {
        
        
    }
    
    
    
    
}
