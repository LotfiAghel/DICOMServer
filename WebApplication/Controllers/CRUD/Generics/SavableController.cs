using System;
using System.Threading.Tasks;
using ClientMsgs;
using Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApplication.Controllers;

public class SavableController<T, TKEY>(DBContext context, IUserService userService)
    : GenericClientCrudController<T, TKEY, DBContext>(context, userService)
    where T : class, ICustomerEntity2, Models.IIdMapper<TKEY>
    where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
{
    [HttpPost("save")]
    public async Task<ObjectContainer<T>> save([FromBody] ObjectContainer<T> data)
    {
        
        if (data.data is null)
            return data;
        data.data.CustomerId = getUserId();
        //if(typeof(T).IsAssignableTo(typeof(ILearnBranch)))
        
        if (data.data.id.Equals(default(TKEY)))
        {
            _db.Add(data.data);
        }
        else
        {
            var z=await _db.FindAsync(data.data.id);
            if (!z.CustomerId.Equals(this.getUserId()))
                return null;
            _db.Entry(z).CurrentValues.SetValues(data.data);
            _db.Entry(z).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        return data;
    }
       
}

public class SavableRemovableController<T, TKEY>(DBContext context, IUserService userService)
    : SavableController<T, TKEY>(context, userService)
    where T : class, ICustomerEntity2, Models.IIdMapper<TKEY>,Models.IRemoveable
    where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
{
    [HttpDelete("{id}")]
    
    override public async Task<ObjectContainer<T>> DeleteEntity([FromRoute] TKEY id)
    {
        T z=await _db.FindAsync(id) as T;
        if (!z.CustomerId.Equals(this.getUserId()))
            return null;

        z.IsRemoved = true;
        
        
        _db.Entry(z).State = EntityState.Modified;
        await _context.SaveChangesAsync();
           
        return new ObjectContainer<T>(z);
    }

}