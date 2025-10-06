using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models;
using ExamPartSession = Models.ExamPartSession;

namespace AdminModels;

public class ExamPartAction:IDbAction<User>
{
    public async Task<User> run(User entity, IServiceProvider Services)
    {
        using (var scope = Services.CreateScope())
        {
            var services = scope.ServiceProvider;
                var context = services.GetRequiredService<IAssetManager>();
                var exp = context.getDbSet<ExamPartSession>();
                await exp.Where(x => x.CustomerId == entity.id && x.isFirst == null).ExecuteUpdateAsync(x =>
                    x.SetProperty(
                        curExmp => curExmp.isFirst,
                        curExmp => exp.Any(prvExp => prvExp.CustomerId == entity.id && prvExp.examId == curExmp.examId && prvExp.SectionType == curExmp.SectionType && prvExp.startTime < curExmp.startTime)
                    )
                );
            
			return entity;
        }

        
    }
}



public class ExamPartAction2:IDbAction<User>
{
    public async Task<User> run(User entity, IServiceProvider Services)
    {
        using (var scope = Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<IAssetManager>();
            var exp = context.getDbSet<Response>();
            var qq=exp.Where(x =>
                    x.examPartSession.CustomerId == entity.id && (x.examPartSession.isFirst != null) &&
                    (x.examPartSession.isFirst.Value))
                .Include(x => x.examPartSession)
                .GroupBy(x => x.QuestionId)
                .Select(x => x.OrderBy(x => x.EnterDate).First());
            
                
            
            return entity;
        }

        
    }
}



public class SectionTypeAction:IAction<SectionType>
{
    public int ddd { get; set; }
    public async Task<SectionType> run(SectionType entity, IServiceProvider Services)
    {
        
            return entity;
        

        
    }
}