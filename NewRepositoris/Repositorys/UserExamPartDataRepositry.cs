using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


public class UserSectionDataRepositry : NUserRepositry<UserSectionData>
{
    public UserSectionDataRepositry(DBContext context, Guid uId) : base(context, uId)
    {
    }
    public async Task<UserSectionData> create(Section section)
    {
        var userSectionData = await _context.userSectionDatas.AsNoTracking().Where(x =>
                x.CustomerId == uId
                && x.sectionId == section.id).FirstOrDefaultAsync();
        bool isNew = false;
        if (userSectionData == null)
        {
            userSectionData = new UserSectionData()
            {
                CustomerId = uId,
                sectionId = section.id,

                lastAnwsers = new Dictionary<int, UserSectionData.QuestionResponseSummary>(),
                firstAnwsers = new Dictionary<int, UserSectionData.QuestionResponseSummary>()
            };
            //_context.examPartUserDatas.Add(examPartUserData);
            isNew = true;
        }
        userSectionData.updatedAt = DateTime.MinValue;
        if (isNew)
            _context.userSectionDatas.Add(userSectionData);
        else
            _context.userSectionDatas.Update(userSectionData);
        return userSectionData;
    }
    public async Task<UserSectionData> addResponseWOS(int sectionId)//WOS = Without Save
    {
        var userSectionDatas = await _context.userSectionDatas.AsNoTracking().Where(x =>
                x.CustomerId == uId
                && x.sectionId == sectionId).FirstOrDefaultAsync();

        if (userSectionDatas == null)
        {
            userSectionDatas = new UserSectionData()
            {
                CustomerId = uId,
                sectionId = sectionId,

                lastAnwsers = new Dictionary<int, UserSectionData.QuestionResponseSummary>(),
                firstAnwsers = new Dictionary<int, UserSectionData.QuestionResponseSummary>()
            };
            _context.userSectionDatas.Add(userSectionDatas);
        }

        return userSectionDatas;
    }
}



public class UserSubjectDataRepositry : NUserRepositry<UserSubjectData>
{
    public UserSubjectDataRepositry(DBContext context, Guid uId) : base(context, uId)
    {
    }
    public async Task<UserSubjectData> create(int subjectId)
    {
        var userSubjectData = await _context.UserSubjectDatas.AsNoTracking().Where(x =>
                x.CustomerId == uId
                && x.subjectId == subjectId

                ).FirstOrDefaultAsync();

        if (userSubjectData != null)
            return userSubjectData;

        userSubjectData = new UserSubjectData()
        {
            CustomerId = uId,
            subjectId = subjectId,
        };

        userSubjectData.updatedAt = DateTime.MinValue;


        _context.UserSubjectDatas.Add(userSubjectData);
        await _context.SaveChangesAsync();


        return userSubjectData;
    }
}


public class UserExamPartDataRepositry : NUserRepositry<UserExamPartData>
{
    public UserExamPartDataRepositry(DBContext context, Guid uId) : base(context, uId)
    {
    }
    public async Task<UserExamPartData> create(int examId, EnglishToefl.Models.ExamPartType t)
    {
        var examPartUserData = await _context.userExamPartDatas.AsNoTracking().Where(x =>
                x.CustomerId == uId
                && x.examId == examId
                && x.examPartType == t
                ).FirstOrDefaultAsync();
        bool isNew = false;
        if (examPartUserData != null)
            return examPartUserData;

        examPartUserData = new UserExamPartData()
        {
            CustomerId = uId,
            examId = examId,
            examPartType = t,

            lastAnwsers = new Dictionary<int, UserSectionData.QuestionResponseSummary>()
        };

        examPartUserData.updatedAt = DateTime.MinValue;


        _context.userExamPartDatas.Add(examPartUserData);
        await _context.SaveChangesAsync();


        return examPartUserData;
    }
    
    
    public  async Task<List<ExamPartSession>> getExamPartSessions(Guid uId, LearnBranch learnBranch)
    {
        var q = _context.ExamPartSessions
            .Where(x=> x.exam.company.learnBranch==learnBranch)
            .Where(x => x.CustomerId == uId);
        var z=await q.Where(x => x.isFirst == null || x.isFirst.Value==true).Take(200).ExecuteUpdateAsync(
            x => x.SetProperty(
                session => session.isFirst,
                session => !q.Any(y => y.examId == session.examId && y.SectionType==session.SectionType && y.startTime < session.startTime)
            )
        );
        var t= await q.Where(x => x.isFirst!=null &&  x.isFirst.Value).OrderBy(x => x.startTime).ToListAsync();
        return t;
    }
}
