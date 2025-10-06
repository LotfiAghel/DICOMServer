using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace WebApplication.Controllers;

[Authorize]
[Route("v1/generic/Models__UserStatics")]
public class UserStaticsController : GenericClientCrudController<Models.Exam, int, DBContext>
{
    public UserStaticsController(DBContext context, IUserService userService) : base(context, userService)
    {
    }
    [HttpGet("{id}/questionsOfType/{type}")]
    public async Task<List<Question>> questionsOfType(int id, EnglishToefl.Models.ExamPartType type)
    {
        return _context.Set<Question>().Where(x => x.section.ExamId == id && x.section.ExamPartType == type)
            .OrderBy(x => x.section.PartOrder)
            .ThenBy(x => x.section.id)
            .ThenBy(x => x.PartOrder).ThenBy(x => x.id).ToList();
    }
        
}