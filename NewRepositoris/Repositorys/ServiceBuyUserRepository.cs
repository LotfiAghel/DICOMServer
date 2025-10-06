using Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using Company = Models.Company;


public class ServiceBuyUserRepository2 : NRepositry<ServiceBuy>
    {
        
        public ServiceBuyUserRepository2(DBContext context):base(context)
            {
                
            }
      
        public async Task<Service> GetService(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            return service;
        }

        public async Task<List<Service>> GetAllServices()
        {
            var services = await _context.Services.Where(x => !x.IsRemoved).AsNoTracking().ToListAsync();
            return services;
        }
        public async Task<List<ServiceGroup>> GetAllServiceGroups()
        {
            var serviceGroups = await _context.ServiceGroups.AsNoTracking().ToListAsync();
            return serviceGroups;
        }


        public async Task<bool> BuyService(Guid userId, int serviceId,decimal amount)
        {
            var service = await _context.Services.FindAsync(serviceId);
            
            var buyedService = await GetAllForUserWithServices(userId);
            var maxValid=DateTime.UtcNow;
            foreach (var s in buyedService)
            {
                if (s.Service.ServiceGroupId == service.ServiceGroupId && s.ValidUntil > maxValid)
                    maxValid=s.ValidUntil;
            }
            
            
            ServiceBuy serviceBuy = new ServiceBuy(service,maxValid,amount);
            serviceBuy.CustomerId = userId;
            _context.ServiceBuys.Add(serviceBuy);
            return await _context.SaveChangesAsync() > 0;

        }

        public async Task<List<ServiceUseLog>> GetAllUseLogUser(Guid userId)
        {
            var useLogs = await _context.ServiceUseLogs.Where(x => x.CustomerId == userId).AsNoTracking().ToListAsync();
            return useLogs;
        }

        public async Task<List<ServiceBuy>> GetAllForUser(Guid userId)
        {
            var buys = await _context.ServiceBuys.Where(x => x.CustomerId == userId).AsNoTracking().ToListAsync();
            return buys;
        }

        public async Task<List<ServiceBuy>> GetAllForUserWithUseLogs(Guid userId)
        {
            var buys = await _context.ServiceBuys.Include(x=>x.ServiceUseLogs).Where(x => x.CustomerId == userId).AsNoTracking().ToListAsync();
            return buys;
        }

        public async Task<List<ServiceBuy>> GetAllForUserWithServices(Guid userId)
        {
            var buys = await _context.ServiceBuys.Include(x => x.Service).Where(x => x.CustomerId == userId).AsNoTracking().ToListAsync();
            return buys;
        }

        public async Task<ServiceBuy> GetByTrakingNumber( long TrackingNumber)
        {
            var buy = await _context.ServiceBuys.Include(x => x.Service)
                .FirstOrDefaultAsync(x => x.TrackingNumber == TrackingNumber);
            return buy;
        }

        public async Task<ServiceBuy> Add(ServiceBuy serviceBuy)
        {
            _context.ServiceBuys.Add(serviceBuy);
            await _context.SaveChangesAsync();
            return serviceBuy;

        }

        public async Task<bool> CheckAndUseSubjectAccess(Guid userId, int companyId)
        {
            
            var now = DateTime.Now;
            var buys = await _context.ServiceBuys.AsNoTracking().Include(x=>x.Service)
                .Where(x => x.CustomerId == userId && x.ValidUntil > now).
                OrderBy(x => x.ValidUntil).ToListAsync();

            foreach (var sBuy in buys)
            {
               if (sBuy.HaveAccessToSubject())
                {
                    
                    return true;

                }
            }
            return false;

        }

        public async Task<bool> CheckAndUseExamAccess(Guid userId, Exam exam,Company comp)
        {
            //var exam = await _context.Exams.FindAsync(examId);
            var est=exam.serviceType;
            if(est==ServiceType.None)
                est=comp.serviceType;

            if (est==ServiceType.None)
                return true;


            var now = DateTime.Now;
            var buys = await _context.ServiceBuys.AsNoTracking().Include(x=>x.ServiceUseLogs).Include(x=>x.Service)
                .Where(x => x.CustomerId == userId && x.ValidUntil > now)
                .Where(s=> s.Service.ServiceGroup.branches.Contains(comp.learnBranch))
                .OrderBy(x => x.ValidUntil).ToListAsync();

            foreach (var sBuy in buys)
            {
                IEnumerable<ServiceUseLog> useLogs;
                if (sBuy.HaveAccessToExam(exam,est, out useLogs))
                {
                    if (!useLogs.Any())
                    {
                        var t = sBuy.UseService(exam,est, userId);
                        if (t!=null)
                        {
                            //_context.Entry(sBuy).State = EntityState.Detached;
                            _context.Add(t);//.State = EntityState.Detached;
                            await _context.SaveChangesAsync();
                        }
                    }
                    return true;

                }
            }
            return false;

        }

        public async Task<bool> CheckAndUseDictionaryAccess(Guid userId)
        {
            var now = DateTime.Now;

            var buys = await _context.ServiceBuys.Where(x => x.CustomerId == userId && x.ValidUntil > now).
                OrderBy(x => x.ValidUntil).ToListAsync();

            foreach (var sBuy in buys)
            {
                if (sBuy.HaveAccessToDictionary())
                {
                    if (sBuy.UseServiceForDictionary(userId))
                    {
                        await _context.SaveChangesAsync();
                    }

                    return true;

                }
            }
            return false;

        }


        public async Task<bool> CheckAndUseLeitnerAccess(Guid userId, LeitnerType leitnerType)
        {

            var now = DateTime.Now;
            var buys = await _context.ServiceBuys.Where(x => x.CustomerId == userId && x.ValidUntil > now).
                OrderBy(x => x.ValidUntil).ToListAsync();

            foreach (var sBuy in buys)
            {
                if (sBuy.HaveAccessToLeitner(leitnerType))
                {
                    if (sBuy.UseServiceForLeitner(userId, leitnerType))
                    {
                        await _context.SaveChangesAsync();
                    }

                    return true;

                }
            }
            return false;

        }

        internal async Task<bool> CheckAndUseLeitnerListAccess(Guid userId)
        {
            var now = DateTime.Now;
            var buys = await _context.ServiceBuys.Where(x => x.CustomerId == userId && x.ValidUntil > now).
                            OrderBy(x => x.ValidUntil).ToListAsync();
            foreach (var sBuy in buys)
            {
                if (sBuy.HaveAccessToLeitnerList())
                {
                    if (sBuy.UseServiceForLeitnerList(userId))
                    {
                        await _context.SaveChangesAsync();
                    }

                    return true;

                }
            }
            return false;

        }

        /*internal async Task<bool> UseServiceExam(Guid userId, Exam exam)
        {
            //var exam = await _context.Exams.FindAsync(examId);
           

            var now = DateTime.Now;
            var buys = await _context.ServiceBuys.Where(x => x.CustomerId == userId && x.ValidUntil > now).
                OrderBy(x => x.ValidUntil).ToListAsync();

            IEnumerable<ServiceUseLog> useLogs;
            foreach (var sBuy in buys)
            {
                if (sBuy.HaveAccessToExam(exam,out useLogs))
                {
                    var t = sBuy.UseService(exam, userId);
                    if (t!=null)
                    {
                        _context.Add(t);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
            }
            return false;

        }*/
    }
