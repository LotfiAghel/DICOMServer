using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientMsgs;
using Data.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using AdminPanel;
using Microsoft.AspNetCore.Http;
using EnglishToefl.Data.Repositories.Users;

public class LeitnerRepositry:NUserRepositry<UserLeitnerCard>
{


    ServiceBuyUserRepository2 serviceBuyRepository;
    private readonly LearnBranch learnBranch;

    public LeitnerRepositry(DBContext context, Guid uId, ServiceBuyUserRepository2 serviceBuyRepository,
        LearnBranch learnBranch):base(context, uId)
    {
        this.learnBranch = learnBranch;
        this.serviceBuyRepository = serviceBuyRepository;
    }
    public async Task<UserLeitnerCard> addWord([FromBody] AddWord word)
    {
        var book2 = word.word.Trim().ToLower();

        var leitnerCard = await _context.LeitnerWords.Where(x => x.content == book2).FirstOrDefaultAsync();
        if (leitnerCard == null)
        {
            leitnerCard = new LeitnerWord() { content = book2 };
            await _context.LeitnerWords.AddAsync(leitnerCard);
        }
        var userLeitnerCard = await _context.LeitnerCardUsers.Where(x=> x.CustomerId==uId).Where(x => x.leitnerWordId == leitnerCard.id).FirstOrDefaultAsync();
        if (userLeitnerCard == null)
        {
            userLeitnerCard = new UserLeitnerCard()
            {
                CustomerId = uId,
                leitnerWordId = leitnerCard.id,
                word=book2,
                createAt = DateTime.UtcNow,
                updateAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                tags = word.tags,
                types = word.litnerTypes,
                sampleText = word.sampleText,
                sampleAudioPath = word.sampleAudioPath,
                sampleTimingMilli = word.sampleTimingMilli,
                sectionId = word.sectionId,


            };
            await _context.LeitnerCardUsers.AddAsync(userLeitnerCard);
        }
        else
        {
            userLeitnerCard.types = word.litnerTypes;
            userLeitnerCard.tags = word.tags;
            userLeitnerCard.updateAt = DateTime.UtcNow;
            userLeitnerCard.updatedAt = DateTime.UtcNow;
            userLeitnerCard.IsRemoved = false;

            userLeitnerCard.sampleText = word.sampleText;
            userLeitnerCard.sampleAudioPath = word.sampleAudioPath;
            userLeitnerCard.sampleTimingMilli = word.sampleTimingMilli;

            _context.Entry(userLeitnerCard).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
        return userLeitnerCard;
    }
    public async Task<Tuple<UserLeitnerCard,bool>> updateWord([FromBody] AddWord word, Guid id = default(Guid))
    {
        var book2 = word.word.Trim().ToLower();

        var leitnerCard = await _context.LeitnerWords.Where(x => x.content == book2).FirstOrDefaultAsync();
        if (leitnerCard == null)
        {
            leitnerCard = new LeitnerWord() { content = book2 };
            await _context.LeitnerWords.AddAsync(leitnerCard);
        }
        UserLeitnerCard userLeitnerCard =null;
        if(id!=default(Guid))
            userLeitnerCard = await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId).Where(x => x.id==id).FirstOrDefaultAsync();
        if (userLeitnerCard == null)
            userLeitnerCard= await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId).Where(x => x.leitnerWordId == leitnerCard.id).FirstOrDefaultAsync();
        bool isNew = false;
        if (userLeitnerCard == null)
        {
            userLeitnerCard = new UserLeitnerCard()
            {
                id = id,
                CustomerId = uId,
                leitnerWordId = leitnerCard.id,
                branches = new List<LearnBranch>(){learnBranch},
                word= book2,
                createAt = DateTime.UtcNow,
                updateAt = DateTime.UtcNow,
                updatedAt= DateTime.UtcNow,
                tags = word.tags,
                types = word.litnerTypes,
                sampleText = word.sampleText,
                sampleAudioPath = word.sampleAudioPath,
                sampleTimingMilli = word.sampleTimingMilli,
                sectionId = word.sectionId,
                leitnerlevel = 1,
                tik8level = 1,
                note = word.note,

            };
            isNew = true;

            await _context.LeitnerCardUsers.AddAsync(userLeitnerCard);
        }
        else
        {

            isNew = userLeitnerCard.IsRemoved;
            if (!userLeitnerCard.types.Contains(word.litnerTypes[0]))
                isNew = true;

            userLeitnerCard.types.AddRange(word.litnerTypes);
            userLeitnerCard.types = userLeitnerCard.types.Distinct().ToList();

            userLeitnerCard.branches ??= new List<LearnBranch>();
            
            userLeitnerCard.branches.Add(learnBranch);
            userLeitnerCard.branches = userLeitnerCard.branches.Distinct().ToList();
            
            if (word.tags != null)
            {
                userLeitnerCard.tags.AddRange(word.tags);
                userLeitnerCard.tags.Distinct();
            }
            userLeitnerCard.word = book2;
            userLeitnerCard.updateAt = DateTime.UtcNow;
            userLeitnerCard.updatedAt = DateTime.UtcNow;
            userLeitnerCard.IsRemoved = false;

            if(word.note != null) 
                userLeitnerCard.note = word.note;

            userLeitnerCard.sampleText = word.sampleText;
            userLeitnerCard.sampleAudioPath = word.sampleAudioPath;
            userLeitnerCard.sampleTimingMilli = word.sampleTimingMilli;

            _context.Entry(userLeitnerCard).State = EntityState.Modified;
        }


        if (id == default(Guid) && isNew && !await UseLeitner())
            return new(null, false);
        await _context.SaveChangesAsync();
        return new(userLeitnerCard, isNew);

    }
    public async Task<bool> UseLeitner()
    {
        return await serviceBuyRepository.CheckAndUseLeitnerAccess(uId, LeitnerType.Leitner);
    }


    public async Task<UserLeitnerCard> updateCard2([FromBody] UserLeitnerCard word)
    {
        

        word.CustomerId=uId;
        var userLeitnerCard = await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId).Where(x => x.id == word.id).FirstOrDefaultAsync();
        _context.Entry(userLeitnerCard).CurrentValues.SetValues(word);
  
        await _context.SaveChangesAsync();
        return userLeitnerCard;

    }


    public async Task updateActions()
    {
        var q2 = await _context.LeitnerCardUsers.Where(x => !x.IsRemoved).Where(x => x.CustomerId == uId).Where(x => x.lastChildsUpdate > x.calcSince).ToListAsync();
        foreach (var leitnerCard in q2)
            await updateActionOfWord(leitnerCard);

    }

    public async Task updateActionOfWord(UserLeitnerCard leitnerCard)
    {


        {
            //var leitnerCard = await _context.LeitnerCardUsers.FindAsync(vm.data.LeitnerCardId);
            var beforThisTime = leitnerCard.ghadimitarinInsert;
            var afterThisTime = leitnerCard.migratedTime;
            if (beforThisTime < leitnerCard.migratedTime)
                beforThisTime = leitnerCard.migratedTime;

            var checkPoint = await _context.LeitnerCardUserCheckPoints
                .Where(x => x.leitnerCardUserId == leitnerCard.id)
                .Where(x => x.calcSince <= beforThisTime)
                .Where(x => x.calcSince >= afterThisTime)
                .OrderBy(x => x.calcSince).LastOrDefaultAsync();


            if (checkPoint == null)
            {
                checkPoint = new UserLeitnerCardCheckPoint()
                {
                    calcSince = afterThisTime,
                    leitnerlevel = 1,
                    tik8level = 1,
                    LeitnerNextTime = DateTime.MinValue,
                    tik8NextTime = DateTime.MinValue,
                    leitnerCardUserId = leitnerCard.id

                };
                await _context.LeitnerCardUserCheckPoints.AddAsync(checkPoint);
                await _context.SaveChangesAsync();
            }

            var actions = await _context.LeitnerUserActions.Where(x => x.LeitnerCardId == leitnerCard.id && x.entryAt > checkPoint.calcSince).OrderBy(x => x.entryAt).ToListAsync();




            var lvl = LeitnerUtils.cacl(checkPoint, actions);


            leitnerCard.clone(lvl.Item1);

            if (lvl.Item2 != null && lvl.Item2.calcSince != checkPoint.calcSince)
            {
                checkPoint.clone(lvl.Item2);
                _context.Entry(checkPoint).State = EntityState.Modified;
            }

            leitnerCard.updateAt = DateTime.UtcNow;
            leitnerCard.updatedAt = DateTime.UtcNow;
            leitnerCard.calcSince = DateTime.UtcNow;






            _context.Entry(leitnerCard).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

    }
    private async Task saveAction0(UserLeitnerAction data)
    {
        await _context.LeitnerUserActions.AddAsync(data);
        await _context.SaveChangesAsync();


        await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId).Where(x => x.id == data.LeitnerCardId).ExecuteUpdateAsync(
            x => x.SetProperty(
                            y => y.ghadimitarinInsert,
                            z => z.ghadimitarinInsert < data.entryAt ? z.ghadimitarinInsert : data.entryAt
                        ).SetProperty(
                 y => y.lastChildsUpdate,
                            DateTime.UtcNow
                )
            );
    }

    public async Task saveAction(UserLeitnerAction data)
    {
        await _context.LeitnerUserActions.AddAsync(data);
        await _context.SaveChangesAsync();
       

        await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId).Where(x => x.id == data.LeitnerCardId).ExecuteUpdateAsync(
            x => x.SetProperty(
                            y => y.ghadimitarinInsert,
                            z => z.ghadimitarinInsert < data.entryAt ? z.ghadimitarinInsert : data.entryAt
                        ).SetProperty(
                 y => y.lastChildsUpdate,
                            DateTime.UtcNow
                )
            );
    }
   
    private async Task<UserLeitnerCard> RemoveFrom0(UserLeitnerCard userLeitnerCard, LeitnerType t)
    {
       
        userLeitnerCard.types.Remove(t);
        if (userLeitnerCard.types.IsNullOrEmpty())
            userLeitnerCard.IsRemoved = true;

        await saveAction(new UserLeitnerAction()
        {
            createdAt = DateTime.UtcNow,
            entryAt = DateTime.UtcNow,
            actionType = t == LeitnerType.Leitner ? UserLeitnerAction.ActionType.RemoveFromLietner : UserLeitnerAction.ActionType.RemoveFromTik8,
            spanMilli = 0,
            LeitnerCardId = userLeitnerCard.id,
        });
        return userLeitnerCard;
    }

    public async Task<UserLeitnerCard> RemoveFrom(Guid id, LeitnerType t)
    {
        var userLeitnerCard = await _context.LeitnerCardUsers.Where(x => x.CustomerId == uId && x.id == id).FirstAsync();
        if (userLeitnerCard.CustomerId != uId)
            throw new Exception();
        var w=await RemoveFrom0(userLeitnerCard, t);
        await _context.SaveChangesAsync();
        return w;
    }

    public async Task<List<UserLeitnerCard>> getDays(GetWordsOfDay data)
    {
        await updateActions();
        var q = _context.LeitnerCardUsers.Where(x => true);
        

        q = q.Where(x => !x.IsRemoved)
            .Where(x => x.CustomerId == uId);
        return await LeitnerUtils.getDays(q, data);
    }

    public async Task<List<UserLeitnerCard>> getAll(GetWordsOfDay data)
    {


        await updateActions();

        var q = _context.LeitnerCardUsers.Where(x => true);
        

        q = q.Where(x => !x.IsRemoved)
            .Where(x => x.CustomerId == uId)/*
            .Where(x => (x.leitnerlevel < 6 && x.types.Contains(LeitnerType.Leitner)) 
                        || 
                        (x.tik8level <= 8 && x.types.Contains(LeitnerType.Tick8) )
                            )*/;




        if (data.leitnerType != null)
        {
            q = q.Where(x => x.types.Contains(data.leitnerType.Value));
            if(data.leitnerType==LeitnerType.Leitner)
                q = q.Where(x => x.leitnerlevel < 6 );
            if (data.leitnerType == LeitnerType.Tick8)
                q = q.Where(x => x.tik8level <= 8);
        }

        if (data.tags != null)
            q = q.Where(x => x.tags.Intersect(data.tags).Any());

        //q = q.Include(x => x.leitnerWord);
        return await q.ToListAsync();
    }
    public async Task<List<UserLeitnerCard>> getAll2(GetWordsOfDay data)
    {


        await updateActions();

        var q = _context.LeitnerCardUsers.Where(x => true);


        q = q.Where(x => !x.IsRemoved)
            .Where(x => x.CustomerId == uId)/*
            .Where(x => (x.leitnerlevel < 6 && x.types.Contains(LeitnerType.Leitner)) 
                        || 
                        (x.tik8level <= 8 && x.types.Contains(LeitnerType.Tick8) )
                            )*/;




        

        if (data.tags != null)
            q = q.Where(x => x.tags.Intersect(data.tags).Any());

        //q = q.Include(x => x.leitnerWord);
        return await q.ToListAsync();
    }

    public async Task<LeitnerWordTag> createTag(CreateTag word)
    {
        var x = new LeitnerWordTag()
        {
            CustomerId=uId,
            title = word.title,
            color = word.color
        };
        _context.leitnerWordTags.Add(x);
        await _context.SaveChangesAsync();
        return x;
    }

    public async Task<int> removeAll(RemoveWords data)
    {
        await updateActions();

        var q = _context.LeitnerCardUsers.Where(x => true);


        q = q.Where(x => !x.IsRemoved)
            .Where(x => x.CustomerId == uId);
        if(!data.removeLearned)
            q=q.Where(x => (x.leitnerlevel < 6 && x.types.Contains(LeitnerType.Leitner))
                             ||
                             (x.tik8level <= 8 && x.types.Contains(LeitnerType.Tick8))
            );

           



        if (data.leitnerType != null)
        {
            q = q.Where(x => x.types.Contains(data.leitnerType.Value));
            if (!data.removeLearned)
            {
                if (data.leitnerType == LeitnerType.Leitner)
                    q = q.Where(x => x.leitnerlevel < 6);
                if (data.leitnerType == LeitnerType.Tick8)
                    q = q.Where(x => x.tik8level <= 8);
            }
        }

        if (data.tags != null)
            q = q.Where(x => x.tags.Intersect(data.tags).Any());

        var words=await q.ToListAsync();
        foreach(var w in words)
        {
            await RemoveFrom0(w, data.leitnerType.Value);
        }
        await _context.SaveChangesAsync();
        //q = q.Include(x => x.leitnerWord);
        /*return await q.ExecuteUpdateAsync(x=> 
            x.SetProperty(x=> x.types,x => x.types.Where(x=> x!= data.leitnerType.Value))
            .SetProperty(x=> x.updateAt, DateTime.UtcNow)
        );;*/
        return words.Count();
    }

    public async Task<UserLeitnerCard> updateCardTags(Guid id, List<ForeignKey2<LeitnerWordTag, Guid>> tags)
    {
        

        var userLeitnerCard = await _context.LeitnerCardUsers.Where(x => x.id == id).FirstOrDefaultAsync();
        if (userLeitnerCard == null || userLeitnerCard.CustomerId!=uId)
        {

            return null;
        }
        userLeitnerCard.tags = tags;
        _context.Entry(userLeitnerCard).State = EntityState.Modified;

        await _context.SaveChangesAsync();
        return userLeitnerCard;
    }

    public async Task<UserLeitnerCard> updateFeild(Guid id, string v, object data)
    {
        var userLeitnerCard = await _context.LeitnerCardUsers.Where(x => x.id == id).FirstOrDefaultAsync();
        if (userLeitnerCard == null || userLeitnerCard.CustomerId != uId)
        {

            return null;
        }
        /*_context.LeitnerCardUsers.Where(x => x.id == id && x.CustomerId == uId).ExecuteUpdateAsync(
            x => x.SetProperty(v, data)
            );*/ //TODO
        typeof(UserLeitnerCard).GetProperty(v).SetValue(userLeitnerCard ,data);
        _context.Entry(userLeitnerCard).State = EntityState.Modified;

        await _context.SaveChangesAsync();
        return userLeitnerCard;
    }

    public async Task<int> updateTag(Guid id, CreateTag word)
    {
        return await _context.leitnerWordTags.Where(x => x.id == id && x.CustomerId == uId).ExecuteUpdateAsync(
            x => x.SetProperty(
                x => x.color,
                word.color
                ).SetProperty(
                x => x.title,
                word.title
                )

            );
    }

    public async Task<int> deleteTag(Guid id)
    {
        return await _context.leitnerWordTags.Where(x => x.id == id && x.CustomerId == uId).ExecuteDeleteAsync();
    }

    public async Task<UserLeitnerWord> setWordState(Guid wordId, UserLeitnerWord.State state)
    {
        var z=await _context.userLeitnerWords.Where(x => x.CustomerId == uId && x.leitnerWordId == wordId).ExecuteUpdateAsync(
            x => x.SetProperty(
                x => x.state,
                state
                )
            );
        if (z == 0)
        {
            _context.userLeitnerWords.Add(new UserLeitnerWord()
            {
                CustomerId = uId,
                leitnerWordId = wordId,
                state = state
            });
            await _context.SaveChangesAsync();
        }
        return await _context.userLeitnerWords.Where(x => x.CustomerId == uId && x.leitnerWordId == wordId).FirstOrDefaultAsync();
    }
}
