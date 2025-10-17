using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data;
using Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AdminServer.Services;
using EnglishToefl.Helper;
using WebApplication.Controllers;
using Tools;
using Models;
using SGS.Core;

namespace AdminPanel
{
    public static class CollectionHelpers
    {
        public static void AddRange<T>(this ICollection<T> destination,
            IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }
    }
    class StringLike
    {
        public string field;
        public string value;
    }
    class IntEq
    {
        public string field;
        public int value;
    }



    public class GenericClientControllerRouteConvention2 : IControllerModelConvention
    {
        public string url;
        public Type type;
        public bool addSwagger = false;
        private string v;
        private List<Type> tmp;

        public GenericClientControllerRouteConvention2(Type type, string url, List<Type> tmp, bool addSwagger = false)
        {
            this.type = type;
            this.url = url;
            this.addSwagger = addSwagger;
            this.tmp = tmp;
        }

        

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType && controller.ControllerType.GetGenericTypeDefinition() == type)
            {
                var genericType = controller.ControllerType.GenericTypeArguments[0];
                var customNameAttribute = genericType.GetCustomAttribute<GeneratedControllerAttribute>();
                if (customNameAttribute == null)
                    return;
                string surl = "";
                if (customNameAttribute.Route != null)
                {
                    surl = url + customNameAttribute.Route;
                }
                else
                {
                    var name = genericType.GetUrlEncodeName().Replace(".", "/");
                    Console.WriteLine(name);
                    surl = url + name;

                }
                controller.Selectors.Add(new SelectorModel
                {

                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(surl))
                });
                /*if (addSwagger && PublicController.docEntity.Contains(genericType))
                {

                    PublicController.crls.Add(new PublicController.PairControler()
                    {
                        url = surl,
                        contrller = controller.ControllerType
                    });
                }/**/
            }
        }
    }

    public class GenericTypeControllerFeatureProvider2 : IApplicationFeatureProvider<ControllerFeature>
    {
        Type type;
        List<Type> types;
        public GenericTypeControllerFeatureProvider2(Type type, List<Type> types = null)
        {
            this.type = type;
            this.types = types;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            if(types!=null)foreach(var candidate in types)
            {
                //Console.WriteLine(candidate.FullName);
                if (candidate.IsSubclassOf(typeof(Entity)))
                    feature.Controllers.Add(
                    type.MakeGenericType(candidate).GetTypeInfo()
                    );
            }
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var currentAssembly in assms)
            {
                try
                {
                    var candidates = currentAssembly.GetExportedTypes().Where(x => x.GetCustomAttributes<GeneratedControllerAttribute>().Any());
                    //var candidates=new List<Type>(){typeof(Dashboard)}

                    foreach (var candidate in candidates)
                    {
                        //Console.WriteLine(candidate.FullName);
                        foreach(var keyType in new Type[] { typeof(int),typeof(string),typeof(Guid)})
                        if (candidate.IsSubclassOf(typeof(IdMapper<>).MakeGenericType(keyType)))
                            feature.Controllers.Add(
                            type.MakeGenericType(new Type[] { candidate , keyType }).GetTypeInfo()
                            );

                    }
                }
                catch
                {

                }
                //var currentAssembly = typeof(GenericTypeControllerFeatureProvider).Assembly;

            }
        }
    }




    //[Authorize]
    public class GenericAdminCrudController<T,TKEY> : ControllerBase
          where T :    class,Models.IIdMapper<TKEY>
         where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
    {
        private readonly DBContext _context;
        private readonly DbSet<T> _db;

        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            SerializationBinder = TypeNameSerializationBinder.gloabl,
            ContractResolver = MyContractResolver.admin,
            DateFormatHandling = DateFormatHandling.IsoDateFormat

        };

         static JsonSerializerSettings settings2 = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            SerializationBinder = TypeNameSerializationBinder.gloabl,
            ContractResolver = new CollectionClearingContractResolver(){tag=CustomIgnoreTag.Kind.ADMIN},
            DateFormatHandling = DateFormatHandling.IsoDateFormat

        };


        private static JsonSerializer serializer = JsonSerializer.Create(settings);

        static GenericAdminCrudController()
        {
            serializer.Converters.Add(new RialConverter());

            serializer.Converters.Add(new ForeignKeyConverter());
            serializer.Converters.Add(new ForeignKey2Converter<string>());
            serializer.Converters.Add(new ForeignKey2Converter<int>());
        }

        IAdminUserService userService;
        public GenericAdminCrudController(DBContext context, IAdminUserService userService)
        {
            _context = context;
            _db = context.dbSet<T>();
            this.userService = userService;
            


        }


        [HttpGet("get")]
        public async Task<ActionResult<JToken>> GetEntitys()
        {
            //Npgsql.NpgsqlDataReader.GetFieldValue
            var x = await _db.ToListAsync();
            return JToken.FromObject(x, serializer);
        }

        [Bind("start,end")]
        public class Instructor
        {

            public int start { get; set; }
            public int end { get; set; }
        }



        [HttpGet]
        public async Task<ActionResult<JToken>> GetEntitys(string range, string sort, string filter)
        {
            if (!await checkPermission<SelectAccess>()) return Forbid();
            var q = _db.Where(x => true);


           
           
            var sql = q.ToQueryString();

            
            try
            {
                if (filter != null)
                {
                    var j = JToken.Parse(filter);
                    try
                    {
                        var ids = j["id"].ToObject<List<TKEY>>();
                        q = q.Where(x => ids.Contains(x.id));
                    }
                    catch { }
                }

            }
            catch
            {

            }

            var range2 = range.convertToRange();
            var sr = sort.convertToSort();
            var total = q.Count<T>();
            q = EntityFrameworkExtensions.addPagination(q, range2, sr, filter);
            var x = await q.ToListAsync();

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {typeof(T).Name}  {range2.Item1}-{range2.Item2}/{total}");
            return JToken.FromObject(x, serializer);
        }



        [HttpPost("getAll")]
        public async Task<ActionResult<JToken>> GetEntitys2([FromBody] IQueryContainer<T> inp)
        {
            DependesyContainer.IServiceProvider = HttpContext.RequestServices;
            if (!await checkPermission<SelectAccess>() && !await checkSearchPermission(inp.query.GetType())
                            ) return Forbid();
            var q = inp != null ? inp.query.run(_db) : _db;// _db.Where(x => true);
            
            //var q2= from u in q select new {u.id};
            q = EntityFrameworkExtensions.addSort<T>(q, null);
            var x = (await q.ToListAsync());//.ConvertAll(x=>new ForeignKey<T>(x.id));
            var total = x.Count();

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"{total}");
            return JToken.FromObject(x, serializer);
            //return x;
        }

        

        [HttpPost("{id}/runAction")]
        public async Task<ActionResult<JToken>> runAction([FromRoute] TKEY id, [FromBody] ObjectContainer<IAction<T>> inp)
        {
            var user = await userService.check(Request);
            if (!await checkPermission<UpdateAccess>()) return Forbid(); //TODO check action Access
            var en = await _db.FindAsync(id);
            var oldData = ff(en);
            await inp.data.run(en, HttpContext.RequestServices);
            _context.Entry(en).State = EntityState.Modified;
            
            _context.dbSetHistory<TKEY>().Add(EntityHistory<TKEY>.Create<T>(en.id,ff(en),user.id,oldData));
            await _context.SaveChangesAsync();
            return JToken.FromObject(en, serializer);
            //return x;
        }
        
        [HttpPost("{id}/runDbAction")]
        public async Task<ActionResult<JToken>> runAction2([FromRoute] TKEY id, [FromBody] ObjectContainer<IDbAction<T>> inp)
        {
            var user = await userService.check(Request);
            if (!await checkPermission<UpdateAccess>()) return Forbid(); //TODO check action Access
            var en = await _db.FindAsync(id);
            await inp.data.run(en, HttpContext.RequestServices);
            
            return JToken.FromObject(en, serializer);
            //return x;
        }


        [HttpPost("{id}/runAction2")]
        public async Task<ActionResult<JToken>> runAction2(TKEY id, [FromBody] IAction<T> inp)
        {
            if (!await checkPermission<ViewAccess>()) return Forbid(); //TODO check action Access
            var en = await _db.FindAsync(id);
            await inp.run(en, HttpContext.RequestServices);
            

            return JToken.FromObject(inp, serializer);
            //return x;
        }




        public class DDD
        {
            public object data { get; set; }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id)
        {
            if (!await checkPermission<ViewAccess>() && !await checkPermission<SelectAccess>()) return Forbid();

            var entity = await _db.FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            return JToken.FromObject(new DDD(){ data = entity }, serializer)["data"];
        }


        [HttpGet("{id}/{propName}")]
        [IgnoreDocAttribute]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id, string propName, string range, string sort, string filter)
        {
            if (!await checkPermission<ViewAccess>() && !await checkPermission<SelectAccess>()) return Forbid(); //TODO check select access detail Entity
            var user = await userService.check(Request);
            var thisEntity = await _db.FindAsync(id);

            if (thisEntity == null)
            {
                return NotFound();
            }

            var range2 = range.convertToRange();
            dynamic thisDotPropertyValue = null;
            Type thisDotPropertyType = null;
            MemberInfo thisDotProperty0=null;
            try
            {
                var thisDotProperty = typeof(T).GetProperty(propName);
                if (thisDotProperty != null)
                {
                    thisDotProperty0 = thisDotProperty;
                   
                    thisDotPropertyType = thisDotProperty.PropertyType;
                    if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        thisDotPropertyValue = _context.Entry(thisEntity)
                            .Collection(propName)
                            .Query();
                    }
                }

            }
            catch
            {

            }
            try
            {
                var thisDotProperty = typeof(T).GetMethod(propName);
                if (thisDotProperty != null)
                {
                    thisDotProperty0 = thisDotProperty;
                    
                    thisDotPropertyType = thisDotProperty.ReturnType;
                    if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        thisDotPropertyValue = thisDotProperty.Invoke(thisEntity, new object[] { HttpContext.RequestServices });
                    }
                    if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        thisDotPropertyValue = thisDotProperty.Invoke(thisEntity, new object[] { HttpContext.RequestServices });
                    }
                }
            }
            catch
            {

            }
            if(thisDotProperty0==null)
                return NotFound();
            var thisDotPropertyItemType = thisDotPropertyType.GetGenericArguments()[0];
            
            if (!await checkPermission<SelectAccess>(thisDotPropertyType.GetGenericArguments()[0])  && !user.checkPermission<SelectAccess>(thisDotProperty0))
                return Forbid();
            
            var total = Queryable.Count(thisDotPropertyValue);//TODO most add filter
            //thisDotPropertyValue = pp[propName](this, thisEntity);
            var sr = sort.convertToSort();
            thisDotPropertyValue = EntityFrameworkExtensions.addPagination(thisDotPropertyValue, range2, sr, filter);

            var x = await EntityFrameworkQueryableExtensions.ToListAsync(thisDotPropertyValue);


            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {thisDotPropertyItemType.Name}  {range2.Item1}-{range2.Item2}/{total}");
            return JToken.FromObject(x, serializer);



        }
    
        [HttpGet("GetHistory/{id}")]
        [IgnoreDocAttribute]
        public async Task<ActionResult<JToken>> GetHistory(TKEY id ,string range, string sort, string filter)
        {
            if (!await checkPermission<ViewAccess>() && !await checkPermission<SelectAccess>()) return Forbid(); //TODO check select access detail Entity
            var thisEntity = await _db.FindAsync(id);

            if (thisEntity == null)
            {
                return NotFound();
            }

            var range2 = range.convertToRange();
            var tps=typeof(T).GetSubClasses(false).Select(x => x.Name).ToList();
            var thisDotPropertyValue0 = _context.dbSetHistory<TKEY>()
                .Where(x => x.entityName == typeof(T).Name && x.entityId.Equals(id)).OrderByDescending(x=> x.createdAt);
            
                 
            
            
           
            


            var total = Queryable.Count(thisDotPropertyValue0);//TODO most add filter
            //thisDotPropertyValue = pp[propName](this, thisEntity);
            var sr = sort.convertToSort();
            //var thisDotPropertyValue = EntityFrameworkExtensions.addPagination(thisDotPropertyValue0, range2, null, filter);

            var x = await EntityFrameworkQueryableExtensions.ToListAsync(thisDotPropertyValue0);
            //x.Insert(0,EntityHistory<TKEY>.Create<T>(thisEntity, Guid.Empty));
            
            for (var i = 0; i < x.Count-1; i++)
            {
                var ii = x[i];
                var ii2 = x[i+1];
                ii.dif=EntityHistory<TKEY>.GetJsonDiff(ii2.data,ii.data);
                
            }
            
            
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" hostory  {range2.Item1}-{range2.Item2}/{total}");
            return JToken.FromObject(x, serializer);

            


        }

        private static JToken ff(T entity)
        {
            return JToken.FromObject(new { data = entity }, serializer)["data"];
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<JToken>> PutEntity(TKEY id, [FromBody] JObject jentity)
        {
            if (!await checkPermission<UpdateAccess>()) return Forbid();

            bool adminWriteBan = typeof(T).GetCustomAttributes(typeof(AdminWriteBan), true).ToList().GetFirst<object, Models.AdminWriteBan>() != null;
            if (adminWriteBan)
                return NoContent();
            var entity = await _db.FindAsync(id);
            var oldData = ff(entity);
            var be = entity;
            if(entity==null)
                return NotFound();
            var entity2 = jentity.ToObject<ObjectContainer<T>>(serializer).data;
            entity2.id = entity.id;
            
            _context.Entry(entity).CurrentValues.SetValues(entity2);
            _context.Entry(entity).State = EntityState.Modified;
            entity=entity2;
            
            //JsonConvert.PopulateObject(jentity.ToString(), entity, settings2);


            //WebApplication.Helpers.AdminUserChecker.check(HttpContext.Request);
            if ( ! id.Equals( entity.id) )
            {
                return BadRequest();
            }
            if (entity is CUAT t)
            {
                //t.createdAt = DateTime.MinValue;
                t.updatedAt = DateTime.UtcNow;
            }
            var user = await userService.check(Request);
            _context.dbSetHistory<TKEY>().Add(EntityHistory<TKEY>.Create<T>(entity.id,ff(entity),user.id,oldData));
            try
            {
                await _context.SaveChangesAsync();
                if(be.GetType()!=entity.GetType())
                    await _db.Where(u => u.id.Equals(entity.id))
                        .ExecuteUpdateAsync(
                            x => x.SetProperty(b => EF.Property<string>(b, "type"), entity.GetType().Name)
                        );
                return ff(entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        async Task<bool> checkSearchPermission(Type t) 
        {
            try
            {
                var user = await userService.check(Request);
                if(user==null)
                    return false;
                var xx = t.GetCustomAttributes<SelectAccess>().ToArray();
                if (xx.IsNullOrEmpty())
                    return false;
                var rolesHaveAccess = xx.Select(x=> x.kinds).Aggregate((l, r) =>
                {
                    var res=new HashSet<AdminUserRole>();
                    res.AddRange(l);
                    res.AddRange(r);
                    return res;
                });
                if (rolesHaveAccess == null || (!rolesHaveAccess.Intersect(user.roles).Any()))
                    return false;
            }
            catch
            {
                return false;
            };
            return true;
        }

        async Task<bool> checkPermission<T2>() where T2 : ACLAtr
        {
            return await checkPermission<T,T2>();
        }
        
        async Task<bool> checkPermission0<T2>(IEnumerable<ACLAtr> atts) where T2 : ACLAtr
        {
            try
            {
                var user = await userService.check(Request);
                if(user==null)
                    return false;
                var xx = atts.OfType<T2>().ToArray();
                if (xx.IsNullOrEmpty())
                    return false;
                var rolesHaveAccess = xx.Select(x=> x.kinds).Aggregate((l, r) =>
                {
                    var res=new HashSet<AdminUserRole>();
                    res.AddRange(l);
                    res.AddRange(r);
                    return res;
                });
                if (rolesHaveAccess == null || (!rolesHaveAccess.Intersect(user.roles).Any()))
                    return false;
            }
            catch
            {
                return false;
            };
            return true;

        }
        async Task<bool> checkPermission<T2>(Type T1) where T2 : ACLAtr
        {
            return await checkPermission0<T2>(T1.GetCustomAttributes<ACLAtr>());
            

        }
        async Task<bool> checkPermission<T1,T2>() where T2 : ACLAtr
        {

            return await checkPermission<T2>(typeof(T1));
        }

        [HttpPost]
        public async Task<ActionResult<JToken>> PostEntity([FromBody] JToken jentity)
        {
            if (!await checkPermission<InsertAccess>())
                            return Forbid();




            var entity = jentity.ToObject< ObjectContainer<T> >(serializer).data;

            if (entity is CUAT c) {
                c.createdAt = DateTime.UtcNow;
            }
            //WebApplication.Helpers.AdminUserChecker.check(HttpContext.Request);
            //var e=data.ToObject<T>();
            _db.Add(entity);
            var user = await userService.check(Request);
            _context.dbSetHistory<TKEY>().Add(EntityHistory<TKEY>.Create<T>(entity.id,ff(entity), user.id,null));
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get" + typeof(T).Name, new { id = entity.id }, ff(entity));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(TKEY id)
        {
            if (!await checkPermission<DeleteAccess>()) return Forbid();
            bool adminWriteBan = typeof(T).GetCustomAttributes(typeof(AdminWriteBan), true).ToList().GetFirst<object, Models.AdminWriteBan>() != null;
            if (adminWriteBan)
                return NoContent();

            var entity = await _db.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _db.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EntityExists(TKEY id)
        {
            return _db.Any(e => e.id.Equals(id));
        }
    }

}
