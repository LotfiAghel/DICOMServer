
using ApiCall;
using ClTool;
using Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Tools;

public class GenericAdminController<T, TKEY> : ApiCall.Service where T : IIdMapper<TKEY>
    where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
{
    public GenericAdminController(WebClient webClient) : base(webClient)
    {
    }
    public async Task<List<T>> getAll()
    {
        return await webClient.fetch<object, List<T>>($"{typeof(T).GetUrlEncodeName()}",
            HttpMethod.Get, null);
    }
    public async Task<List<T2>> getAllDetailClass<T2>(T id, string property)
    {
        return await webClient.fetch<object, List<T2>>($"{typeof(T).GetUrlEncodeName()}/{id.id}/{property}",
            HttpMethod.Get, null);
    }
    public async Task<List<T2>> getAllDetailClass2<T2>(T id, Expression<Func<T, ICollection<T2>>> action)
    {


        try
        {
            var expression = (MemberExpression)action.Body;
            var property = expression.Member as PropertyInfo;
            return await getAllDetailClass<T2>(id, property.Name);

        }
        catch (Exception e)
        {

        }



        return null;




    }
    public async Task<List<object>> getAllDetailClass2<T2>(object id, string property)
    {
        return await webClient.fetch<object, List<object>>($"{typeof(T).GetUrlEncodeName()}/{id}/property",
            HttpMethod.Get, null);
    }

    public async Task <T> post(T content)
    {
        return await webClient.fetchForDriveds<T, T>($"{typeof(T).GetUrlEncodeName()}",
            HttpMethod.Post, content);
    }
    public async Task<T> Put(T content)
    {
        return await webClient.fetchForDriveds<T, T>($"{typeof(T).GetUrlEncodeName()}/{content.id}",
            HttpMethod.Put, content);
    }

    public async Task<T> PostOrPut(T content)
    {
        if (content.id.Equals(default(TKEY)))
            return await post(content);
        return await Put(content);
    }

    public async Task<List<T>> GetEntitys2(IQueryContainer<T> inp)
    {
            return await webClient.fetch<IQueryContainer<T>, List<T>>($"{typeof(T).GetUrlEncodeName()}/getAll",
               HttpMethod.Post, inp);
        }
    public async Task<T> get(TKEY id)
    {
        return await webClient.fetch<object, T>($"{typeof(T).GetUrlEncodeName()}/{id}",
           HttpMethod.Get, null);
    }
}



