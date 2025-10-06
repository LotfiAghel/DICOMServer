using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Data;
using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Controllers;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Models.Notifications;

namespace EnglishToefl.Controllers.APIControllers
{
    [Authorize]
    [Route("v1/news")]
    public class NewsController : UsersBaseController
    {
        internal readonly DBContext _context;
        public NewsController(DBContext context,IUserService userService) : base(userService)
        {
            this._context = context;
        }

        [HttpGet("read/{id}")]
        public async Task<int> click([FromRoute] Guid id)
        {
            return await _context.userExtraDatas.Where(x => x.id == this.getUserId()).ExecuteUpdateAsync(
                x => x.SetProperty(
                    x=> x.readedNews,
                    x=> x.readedNews.Concat(new List<Guid>() { id})
                    )
                );
            
        }

        [HttpGet("getSeen")]
        public async Task<UserExtraData> getSeen()
        {
            var tmp= await _context.userExtraDatas.Where(x => x.id == this.getUserId()).FirstOrDefaultAsync();
            if (tmp == null)
            {
                tmp = new UserExtraData() { id = this.getUserId() };
                _context.userExtraDatas.Add(tmp);
                await _context.SaveChangesAsync();
            }
            return tmp;


        }
        

        [HttpGet("closePopup/{id}")]
        public async Task<int> closePopup([FromRoute] Guid id)
        {
            return await _context.userExtraDatas.Where(x => x.id == this.getUserId()).ExecuteUpdateAsync(
                x => x.SetProperty(
                    x => x.closedPopup,
                    x => x.closedPopup.Concat(new List<Guid>() { id })
                    )
                );

        }


        [HttpGet("platfrom/{plat}")]
        public async Task<List<UserNotif>> platfrom([FromRoute] Platform plat)
        {
            var tmp = await _context.userInboxItems.Where(x => x.CustomerId == this.getUserId()
            && !x.IsRemoved
            ).OrderBy(x => x.startDate).ToListAsync();
            var res = tmp.ConvertAll<UserNotif>(x => x);
            var rd = (await this.getNUser()).registerDate;
            var tmp2 = await _context.news.Where(x => (x.platforms==null || x.platforms.Contains(plat))
                                                      && !x.IsRemoved
                                                      && (x.learnBranchs==null || x.learnBranchs.Contains(getLearnBranch()))
                //&& x.userRegisterMinDate <= rd
                //&& x.userRegisterMaxDate >= rd
                //&& x.endDate > DateTime.UtcNow
                //&& x.startDate < DateTime.UtcNow

            ).OrderBy(x => x.startDate).ToListAsync();
            res.AddRange(tmp2);
            return res.OrderBy(x => x.startDate).ToList();
        }
    }

}