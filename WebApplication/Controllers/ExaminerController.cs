using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientMsgs;
using Data.Data;
using EnglishToefl.Data;
using EnglishToefl.Data.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ViewGeneratorBase;
using WebApplication.Controllers;

namespace WebApplication.Controllers;

[Route("v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class ExaminerController(
    DBContext context,
    IUserService userService) : UsersBaseController(userService)
{
    [HttpGet]
    public async Task<User> changeExaminerFinder()
    {
        var thisUser = await context.users.FindAsync(getUserId());
        thisUser!.examinerFinder = Guid.NewGuid();
        context.Entry(thisUser).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return thisUser;
    }

    [HttpGet]
    public async Task<List<UserExaminer>> getExaminerRequests()
    {
        return await context.userExaminers.Where(x=> x.userId == getUserId() && !x.IsRemoved).ToListAsync();
        
    }
    [HttpGet]
    public async Task<List<UserExaminer>> getClientRequests()
    {
        return await context.userExaminers.Where(x=> x.examinerId == getUserId() && !x.IsRemoved).ToListAsync();
        
    }
        
        
    [HttpPost]
    public async Task<UserExaminer> createExaminerRequests([FromBody] ExaminerRequest request)
    {
        var examiner = await context.users.Where(x=> x.examinerFinder == request.examinerFinder).FirstOrDefaultAsync();
        if (examiner == null)
        {
            return null;
        }
        var result=await context.userExaminers.Where(x=> x.examinerId==examiner.id && !x.IsRemoved && x.userId==getUserId()).FirstOrDefaultAsync();
        if (result != null) return result;
        result = new UserExaminer()
        {
            createdAt = DateTime.UtcNow,
            examinerId = examiner.id,
            userId = getUserId(),
        };
        context.userExaminers.Add(result);
        await context.SaveChangesAsync();
        return result;
    }
    
    
    [HttpPost]
    public async Task<UserExaminer> delete([FromBody] ObjectContainer<Guid> request)
    {
        var r=await context.userExaminers.FindAsync(request.data);
        if (r != null && (r.examinerId != getUserId() || r.userId != getUserId()))
            return null;
        r!.IsRemoved = true;
        context.Entry(r).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return r;
    }
    
    
    [HttpPost]
    public async Task<UserExaminer> acceptClient([FromBody] ObjectContainer<Guid> request)
    {
        var r=await context.userExaminers.FindAsync(request.data);
        if (r != null && r.examinerId != getUserId())
            return null;
        r!.accepted = true;
        context.Entry(r).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return r;
    }
    
    [HttpPut]
    public async Task<ObjectContainer<String>> uploadVoice([FromForm] UploadReviseFile viewModel)
    {
        var uId = this.getUserId();
        var res = await context.responseAdjusts.FindAsync(viewModel.reviseId);
        
        if (res.examinerId != uId)
            return null;
        
        var path = Path.Combine(JsonBase64File.UserUploadFolderPath,"revise", uId.ToString(),res.id.ToString());
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var name = Guid.NewGuid();



        if (viewModel.File?.Length > 0)
        {
            //large file
            if (viewModel.File?.Length > 200 * 1024 * 1024)
            {
                throw new Exception("Big file for voice upload");
            }
            var fn = $"{path}/{name}.mp3";
            await using (var stream = new FileStream(fn, FileMode.Create))
            {
                if (viewModel.File != null) 
                    await viewModel.File.CopyToAsync(stream);
            }

            return new ObjectContainer<string>($"{name}.mp3");
        }

        return null;
    }
    public async Task<ObjectContainer<AResponseAdjust>> saveRevise([FromBody] ObjectContainer<AResponseAdjust> vm)
    {
        //vm.exam
        var uId = this.getUserId();
        var rep = new ReviseRepo(context, uId);
        
        vm.data.examinerId = uId;
        if (vm.data.id!=Guid.Empty)
        {
            var res = await context.responseAdjusts.FindAsync(vm.data.id);
            if (res != null)
            {
                context.Entry(res).CurrentValues.SetValues(vm.data);
                context.Entry(res).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return new ObjectContainer<AResponseAdjust>(res);
            }
        }
        context.Add(vm.data);
        await context.SaveChangesAsync();
        return vm;
        
    }
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserExaminer>> getConnection([FromRoute] Guid userId)
    {
        var con=await context.userExaminers.Where(x=>
                
            x.userId==userId && x.examinerId==this.getUserId()
                             &&x.accepted && !x.IsRemoved
        ).OrderByDescending(x=> x.createdAt).FirstOrDefaultAsync();
            
                 
        return con;
    }


}