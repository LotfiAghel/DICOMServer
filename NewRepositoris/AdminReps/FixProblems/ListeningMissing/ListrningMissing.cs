using Data.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace NewRepositoris.AdminReps.FixProblems.ListeningMissing
{
    public class ListrningMissing
    {
        internal readonly DBContext _context;
        



        public ListrningMissing(DBContext context)
        {
            this._context = context;
        }


        public IQueryable<Response> getResponseWithProblem()
        {
            return _context.Responses.Where(x =>
                x.question.section.ExamPartType==EnglishToefl.Models.ExamPartType.Listening
                && x.examPartSession.SectionType == EnglishToefl.Models.ExamPartType.Reading
                && x.question.section.ExamPartType != x.examPartSession.SectionType
            );
        }

        public async Task<int> fixProblem()
        {
            var examPartSessions = await getResponseWithProblem()//.Where(x => !x.examPartSession.examSession.parts.Where(x => x.SectionType == EnglishToefl.Models.ExamPartType.Listening).Any())
                .Select(x => x.examPartSession
                    ).Distinct().ToListAsync();
            int res = 0;
            foreach (var examPartSession in examPartSessions)
            {
                var firstListingResponse=await _context.Responses.Where(x => x.examPartSessionId == examPartSession.id && x.question.section.ExamPartType == EnglishToefl.Models.ExamPartType.Listening)
                    .OrderBy(x => x.EnterDate).FirstOrDefaultAsync();
                if (firstListingResponse == null)
                    continue; //may null becuse examPartSessions not distinct

                var listingExamPart =_context.ExamPartSessions.Where(x=> x.examSessionId==examPartSession.examSessionId && x.SectionType==EnglishToefl.Models.ExamPartType.Listening).FirstOrDefault();
                
                if (listingExamPart==null)
                {
                    listingExamPart=new ExamPartSession()
                    {
                        CustomerId= examPartSession.CustomerId,
                        SectionType=EnglishToefl.Models.ExamPartType.Listening,
                        examId = examPartSession.examId,
                        examSessionId = examPartSession.examSessionId,
                        startTime = firstListingResponse.EnterDate,
                    };
                    _context.ExamPartSessions.Add(listingExamPart);
                    await _context.SaveChangesAsync();

                }
                res += await getResponseWithProblem().Where(x => x.examPartSessionId == examPartSession.id).ExecuteUpdateAsync(
                        x => x.SetProperty(x => x.examPartSessionId, listingExamPart.id)
                    );
                await _context.ExamPartSessions.Where(x=>x.id== examPartSession.id)
          .ExecuteUpdateAsync(x =>
                  x.SetProperty(
                      x => x.score,
                      s => _context.Responses.Where(x => x.examPartSessionId == s.id).GroupBy(x => x.QuestionId).Select(x => x.OrderByDescending(x => x.EnterDate).First()).Sum(x => x.Score)
                  )
              );

            }
            return res;
        }


    }
}
