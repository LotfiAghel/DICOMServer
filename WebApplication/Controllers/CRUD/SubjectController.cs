using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApplication.Controllers;

[Authorize]
[Route("v1/generic/Models__Subject")]
public class SubjectController : GenericClientCrudController<Models.Subject, int, DBContext>
{
    public SubjectController(DBContext context, IUserService userService) : base(context, userService)
    {
    }
    [HttpGet("{id}/sections2/{companyId}")]
    public async Task<List<Section>> sections([FromRoute] int id,[FromRoute] int companyId)
    {
            
        return await _context.SectionSubjects.Where(x =>  x.subjectId == id && x.section.exam.CompanyId==companyId).OrderBy(x=> x.section.exam.PartOrder).Select(x =>x.section).ToListAsync();
          
    }
    
    
    [HttpGet("{id}/sections3/{companyId}")]
    public async Task<List<Section>> sections3([FromRoute] int id,[FromRoute] int companyId)
    {
        
        return await _context.Sections.Where(x => x.subjectId!=null && x.subjectId == id && x.exam.CompanyId==companyId).OrderBy(x=> x.exam.PartOrder).Select(x =>x).ToListAsync();
          
    }
        
}