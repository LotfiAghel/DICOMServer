using Data.Data;
using EnglishToefl.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace NewRepositoris.AdminReps
{
    public class WritngTextNullData
    {
        public int ted = 0;

    }
    public class WritngTextNull
    {
        internal readonly DBContext _context;
        internal readonly ApplicationDbContext _old;




        public WritngTextNull(ApplicationDbContext old,DBContext context)
        {
            this._context = context;
            this._old = old;
        }


        public IQueryable<Response> getResponseWithProblem()
        {
            return _context.Responses.Where(x => x.GetType() == typeof(WritingResponse));
        }

        public async Task<int> fixProblem(WritngTextNullData o)
        {
            var responses = await getResponseWithProblem().ToListAsync();
            int i = 0,result=0;
            foreach (var response in responses) {
                var oldRes = await _old.Responses.Where(x => x.id == response.id).FirstOrDefaultAsync();
                if (oldRes != null)
                {
                    ++i;
                    ++result;
                    o.ted = result;
                    (response as WritingResponse).text = oldRes.Content;
                    (response as WritingResponse).Content = oldRes.Content;
                    _context.Entry(response).State = EntityState.Modified;
                    if (i > 100)
                    {
                        i = 0;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            await _context.SaveChangesAsync();
            return result;
        }


    }
}
