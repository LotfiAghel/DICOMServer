
using ApiCall;
using ClTool;
using Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tools;


public class AdminUserController : ApiCall.Service
{
    public AdminUserController(WebClient webClient) : base(webClient)
    {
    }

    

    public async Task<AdminMsg.LoginResponse> login(AdminMsg.LoginRequest code)
    {
        return await webClient.fetch<AdminMsg.LoginRequest, AdminMsg.LoginResponse>("adminUser/login",
            HttpMethod.Post, code);
    }
    
    public async Task<object> migrate()
    {
        return await webClient.fetch<object, object>("adminUser/migrate",
            HttpMethod.Get, null);
    }
    
    public async Task<object> migrateExam( int examId)
    {
        return await webClient.fetch<object, object>($"adminUser/migrateExam/{examId}",
            HttpMethod.Get, null);
        
    }
}
public class GenericContoler : ApiCall.Service
{
    public GenericContoler(WebClient webClient) : base(webClient)
    {
    }
    public async Task<List<T2>> getAllDetailClass<T, T2>(T en, string property)
        where
         T : IEntity0
    {
        return await webClient.fetch<object, List<T2>>($"v1/generic/{typeof(T).GetUrlEncodeName()}/{en.getId()}/{property}",
            HttpMethod.Get, null);
    }
    public async Task<List<T2>> getAllDetailClass2<T, T2>(T id, Expression<Func<T, ICollection<T2>>> action)
        where
         T : IEntity0
    {
        try
        {
            var expression = (MemberExpression)action.Body;
            var property = expression.Member as PropertyInfo;
            return await getAllDetailClass<T, T2>(id, property.Name);

        }
        catch (Exception e)
        {

        }



        return null;




    }
}



