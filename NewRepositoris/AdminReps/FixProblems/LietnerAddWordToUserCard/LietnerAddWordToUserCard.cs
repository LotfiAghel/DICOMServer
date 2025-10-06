using Data.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace NewRepositoris.AdminReps.FixProblems.ListeningMissing
{
    public class LietnerAddWordToUserCard
    {
        internal readonly DBContext _context;
        



        public LietnerAddWordToUserCard(DBContext context)
        {
            this._context = context;
        }


        public async Task<int> fixProblem()
        {

            return await _context.LeitnerCardUsers.Include(x => x.leitnerWord).ExecuteUpdateAsync(
                        x => x.SetProperty(
                            x => x.word,
                            x => _context.LeitnerWords.Where(y=> y.id==x.leitnerWordId).First().content
                            )
                    );

            return await _context.LeitnerCardUsers.Include(x=>x.leitnerWord).ExecuteUpdateAsync(
                        x => x.SetProperty(
                            x=>x.word,
                            x=> x.leitnerWord.content
                            )
                    );

            
        }


    }
}
