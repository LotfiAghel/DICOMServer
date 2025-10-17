using System;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace WebApplication.Controllers
{
    public static class QueryExtra
    {

        public static Expression<Func<T2, T3>> GetParm<T2, T3>(string filedName)
        {
            var param = Expression.Parameter(typeof(T2), "p");
            return Expression.Lambda<Func<T2, T3>>(
                    Expression.Property(param, filedName),
                param
            );
        }
        /*public static object GetParm2<T2, T3>(string filedName)
        {
            var paramType = typeof(T).GetProperty(filedName).GetType();

            var param = Expression.Parameter(typeof(T), "p");
            var funcType = typeof(Func<>).MakeGenericType(new Type[] { typeof(T), paramType });
            var LambdaType = typeof(Expression.Lambda<>).MakeGenericType(new Type[] { funcType });
            return Activator.CreateInstance(LambdaType, new object[] { Expression.Property(param, filedName) });
            return Expression.Lambda<Func<T2, T3>>(
                    Expression.Property(param, filedName),
                param
            );
        }/**/
        public static Expression<Func<T2, bool>> Like<T2>(Expression<Func<T2, string>> prop, string keyword)
        {
            var concatMethod = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });
            return Expression.Lambda<Func<T2, bool>>(
                Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Like),
                    null,
                    Expression.Constant(EF.Functions),
                    prop.Body,
                    Expression.Add(
                        Expression.Add(
                            Expression.Constant("%"),
                            Expression.Constant(keyword),
                            concatMethod),
                        Expression.Constant("%"),
                        concatMethod)),
                prop.Parameters);
        }

        public static Expression<Func<T2, bool>> Like2<T2>(Expression<Func<T2, string>> prop, string keyword)
        {
            return Expression.Lambda<Func<T2, bool>>(
                Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Like),
                    null,
                    Expression.Constant(EF.Functions),
                    prop.Body,
                    Expression.Constant(keyword)),
                prop.Parameters);
        }
        public static Expression<Func<T2, bool>> Equals<T2>(Expression<Func<T2, string>> prop, string keyword)
        {
            return Expression.Lambda<Func<T2, bool>>(
                Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Like),
                    null,
                    Expression.Constant(EF.Functions),
                    prop.Body,
                    Expression.Constant(keyword)),
                prop.Parameters);
        }
        public static Expression<Func<T2, bool>> Equals2<T2, TKEY>(Expression<Func<T2, TKEY>> prop, TKEY keyword)
        {
            return Expression.Lambda<Func<T2, bool>>(
                Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Equals),
                    null,
                    Expression.Constant(EF.Functions),
                    prop.Body,
                    Expression.Constant(keyword)),
                prop.Parameters);
        }

    }




}


