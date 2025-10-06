using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Tools;

using Microsoft.AspNetCore.Authorization;

using System;
using AdminPanel;
using ClientMsgs;
using SGS.Core;



namespace WebApplication.Controllers
{
    
    public class GenericClientControllerRouteConvention1 : IControllerModelConvention
    {
        public string url;
        public Type type;
        public bool addSwagger = false;
        public GenericClientControllerRouteConvention1(Type type, string url, bool addSwagger = false)
        {
            this.type = type;
            this.url = url;
            this.addSwagger = addSwagger;
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
                    var name = genericType.Name.Replace(".", "/");
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

    public class GenericTypeControllerFeatureProvider1 : IApplicationFeatureProvider<ControllerFeature>
    {
        Type type;
        public GenericTypeControllerFeatureProvider1(Type type)
        {
            this.type = type;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {

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
                        if (candidate.IsSubclassOf(typeof(Entity)))
                            feature.Controllers.Add(
                            type.MakeGenericType(candidate).GetTypeInfo()
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


    public class GenericClientControllerRouteConvention : IControllerModelConvention
    {
        public string url;
        public List<Type> types;
        public bool addSwagger = false;
        private string v;


        public GenericClientControllerRouteConvention(string url, List<Type> types, bool addSwagger = false)
        {
            this.types = types;
            this.url = url;
            this.addSwagger = addSwagger;

        }



        public void Apply(ControllerModel controller)
        {
            if (!types.Contains(controller.ControllerType))
                return;
            string surl = "";

            var customNameAttribute2 = controller.ControllerType.GetCustomAttribute<RouteAttribute>();
            if (customNameAttribute2 != null)
            {
                return;
                surl = customNameAttribute2.Template;
                controller.Selectors.Add(new SelectorModel
                {

                    AttributeRouteModel = new AttributeRouteModel(customNameAttribute2)
                });
                
            }
             
            {
                var genericType = controller.ControllerType.GenericTypeArguments[0];
                var customNameAttribute = genericType.GetCustomAttribute<GeneratedControllerAttribute>();
                //if (customNameAttribute == null)
                //    return;

                if (customNameAttribute == null || customNameAttribute.Route == null)
                {
                    var name = genericType.GetUrlEncodeName().Replace(".", "/");
                    Console.WriteLine(name);
                    surl = url + name;
                    Console.WriteLine(surl);
                }
                else
                {
                    surl = url + customNameAttribute.Route;
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


    public class GenericClientControllerRouteConvention2 : IControllerModelConvention
    {
        public string url;
        public Type type;
        public bool addSwagger = false;
        private string v;

        List<Type> types;
        public GenericClientControllerRouteConvention2(Type type, string url, List<Type> types, bool addSwagger = false)
        {
            this.type = type;
            this.url = url;
            this.addSwagger = addSwagger;
            this.types = types;
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




    public class GenericTypeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {

        List<Type> types;
        public GenericTypeControllerFeatureProvider(List<Type> types = null)
        {
            this.types = types;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var candidate in types)
            {

                feature.Controllers.Add(
                 candidate.GetTypeInfo()
                );
            }

        }
    }


    public abstract class GenericClientCrudController : UsersBaseController
    {
        public static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            SerializationBinder = TypeNameSerializationBinder.gloabl,
            ContractResolver = MyContractResolver2.client2,
            DateFormatHandling = DateFormatHandling.IsoDateFormat

        };




        public static JsonSerializer serializer = JsonSerializer.Create(settings);
        static GenericClientCrudController()
        {
            serializer.Converters.Add(new RialConverter());

            serializer.Converters.Add(new ForeignKeyConverter());
            serializer.Converters.Add(new ForeignKey2Converter<string>());
            serializer.Converters.Add(new ForeignKey2Converter<int>());
        }
        protected GenericClientCrudController(IUserService userService) : base(userService)
        {
        }
    }

    //[Authorize]
    [AllowAnonymous]
    public class GenericClientCrudController<T, TKEY, DB> : GenericClientCrudController
          where T : class, Models.IIdMapper<TKEY>
         where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        where DB : DbContext
    {
        protected readonly DB _context;
        protected readonly DbSet<T> _db;


        //IUserService userService;

        public GenericClientCrudController(DB context, IUserService userService) : base(userService)
        {
            _context = context;
            _db = context.Set<T>();

         
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<JToken>> GetEntitys(string range, string sort, string filter)
        {
            if (!await checkPermission<SelectAccess>()) return Unauthorized();


            
            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);
            q = EntityFrameworkExtensions.addSort(q, null);
            //q = EntityFrameworkExtensions.addPagination(q, null, null, filter);
            //Npgsql.NpgsqlDataReader.GetFieldValue
            var x = await q.ToListAsync();
            return JToken.FromObject(x, serializer);
        }




        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<JToken>> GetEntitysAll(string range, string sort, string filter)
        {
            if (!await checkPermission<SelectAccess>()) return Unauthorized();


           

            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);
            
            
            
            
            q = EntityFrameworkExtensions.addSort(q, null);

            
            

            

            //q = EntityFrameworkExtensions.addPagination(q, null, null, filter);
            //Npgsql.NpgsqlDataReader.GetFieldValue
            var x = await q.ToListAsync();
            return JToken.FromObject(x, serializer);
        }



        [HttpPost("getIds")]
        public async Task<ActionResult<JToken>> getIds([FromBody] GetIds<TKEY> ids)
        {
            if (!await checkPermission<SelectAccess>()) return Unauthorized();


            

            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);
            //q = EntityFrameworkExtensions.addPagination(q, null, null, filter);
            //Npgsql.NpgsqlDataReader.GetFieldValue
            var x = await q.Where(x=> ids.ids.Contains(x.id)).ToListAsync();
            return JToken.FromObject(x, serializer);
        }


        [Bind("start,end")]
        public class Instructor
        {

            public int start { get; set; }
            public int end { get; set; }
        }




        [HttpPost("getAll")]
        public async Task<ActionResult<JToken>> GetEntitys2([FromBody] IQueryContainer<T> inp)
        {
            if (!await checkPermission<SelectAccess>()) return Unauthorized();

            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);

            q = inp != null ? inp.query.run(q) : q;// _db.Where(x => true);
            //var q2= from u in q select new {u.id};
            var x = (await q.ToListAsync());//.ConvertAll(x=>new ForeignKey<T>(x.id));
            var total = x.Count();

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"{total}");
            return JToken.FromObject(x, serializer);
            //return x;
        }

        [HttpGet("afterUpdate/{updateAt}")]
        public async Task<ActionResult<JToken>> afterUpdate([FromRoute] long updateAt)
        {
            if (!await checkPermission<SelectAccess>()) return Unauthorized();

            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);

            if (!typeof(T).IsAssignableTo(typeof(IU)))
                return JToken.FromObject("notOk", serializer);

            q=q.Where(x => (x as IU).updatedAt >= updateAt.milliSecToUtc());
            var x = (await q.ToListAsync());//.ConvertAll(x=>new ForeignKey<T>(x.id));
            var total = x.Count();

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"{total}");
            return JToken.FromObject(x, serializer);
            //return x;
        }









        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id)
        {
            if (!await checkPermission<ViewAccess>()) return Unauthorized();

            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);
            var entity = await q.Where(x => x.id.Equals(id)).FirstAsync();

            if (entity == null)
            {
                return NotFound();
            }
            //Response.Headers.Add("Cache-Control", "public,max-age=3600");
            return JToken.FromObject(new { data = entity }, serializer)["data"];
            //return JToken.FromObject(entity, serializer);
        }








        [HttpPut("{id}")]
        public async Task<ActionResult<JToken>> PutEntity(TKEY id, [FromBody] JObject jentity)
        {
            return Unauthorized();
            if (!await checkPermission<UpdateAccess>()) return Unauthorized();

            bool adminWriteBan = typeof(T).GetCustomAttributes(typeof(AdminWriteBan), true).ToList().GetFirst<object, Models.AdminWriteBan>() != null;
            if (adminWriteBan)
                return NoContent();
            var entity = await _db.FindAsync(id);
            if (entity == null)
                return NotFound();

            JsonConvert.PopulateObject(jentity.ToString(), entity, settings);


            //WebApplication.Helpers.AdminUserChecker.check(HttpContext.Request);
            if (!id.Equals(entity.id))
            {
                return BadRequest();
            }
            if (entity is Id4Entity)
            {
                (entity as Id4Entity).createdAt = DateTime.MinValue;
                (entity as Id4Entity).updatedAt = DateTime.MinValue;
            }

            _context.Entry(entity).State = EntityState.Modified;
            //_context.dbSetHistory<TKEY>().Add(EntityHistory<TKEY>.Create(entity));
            try
            {
                await _context.SaveChangesAsync();
                return JToken.FromObject(entity, serializer);
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

        async Task<bool> checkPermission<T2>() where T2 : ACLAtr
        {
            var nuser=await getNUser();
            if (nuser == null && !typeof(T).GetCustomAttributes<PublicClass>().Any())
                return false;
            return typeof(T2) == typeof(ViewAccess) || typeof(T2) == typeof(SelectAccess);

        }

        [HttpPost]
        public async Task<ActionResult<JToken>> PostEntity([FromBody] JToken jentity)
        {
            return Unauthorized();


            

            var entity = jentity.ToObject<T>(serializer);
            var zl = typeof(T).GetCustomAttributes<Attribute>().Where(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(FroceAction<>));
            if(zl.Count()==0)
                return NotFound();

            foreach (var z in zl)
            {
                object t = z.GetType().GenericTypeArguments[0].GetConstructor(new Type[] { }).Invoke(new object[] { });
                //if(t.GetType().IsInstanceOfType(typeof(IQuery<>)) && t.GetType().GetGenericTypeDefinition() == typeof(IQuery<>))
                {
                    var f = t.GetType().GetMethod("run").MakeGenericMethod(new Type[] { typeof(T) });
                    entity = f.Invoke(t, new object[] { entity }) as T;
                }
                //q = t.run(q);//have probelm with IOuery only work with IQuery2
            }
            //WebApplication.Helpers.AdminUserChecker.check(HttpContext.Request);
            //var e=data.ToObject<T>();
            _db.Add(entity);
            //_context.dbSetHistory<TKEY>().Add(EntityHistory<TKEY>.Create(entity));
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get" + typeof(T).Name, new { id = entity.id }, JToken.FromObject(entity, serializer));
        }


        [HttpDelete("{id}")]
        virtual public  async Task<ObjectContainer<T>> DeleteEntity(TKEY id)
        {
            throw new Exception("not removable");
            /*if (!await checkPermission<DeleteAccess>()) return Unauthorized();
            bool adminWriteBan = typeof(T).GetCustomAttributes(typeof(AdminWriteBan), true).ToList().GetFirst<object, Models.AdminWriteBan>() != null;
            if (adminWriteBan)
                return new ObjectContainer<T>(null);

            var entity = await _db.FindAsync(id);
            if (entity == null)
            {
                return new ObjectContainer<T>(null);
            }

            _db.Remove(entity);
            await _context.SaveChangesAsync();

            return new ObjectContainer<T>(entity);*/
        }

        private bool EntityExists(TKEY id)
        {
            return _db.Any(e => e.id.Equals(id));
        }
        class CacheTimer
        {
            public JToken data;
            public DateTime time;
        }
        //static Dictionary<string, CacheTimer> cache = new();

        [HttpGet("{id}/{propName}")]
        [AllowAnonymous]
        [IgnoreDocAttribute]
        //[ResponseCache(Duration = 5*60, Location = ResponseCacheLocation.Any, NoStore = false)]
        //[ResponseCache(VaryByHeader = "User-Agent", Duration = 30)]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id, string propName, string range, string sort, string filter)
        {
            if (!await checkPermission<ViewAccess>()) return Unauthorized(); //TODO check select access detail Entity
            var q = _db.Where(x => true);
            q = EntityFrameworkExtensions.addSecurityFilter<T>(q, HttpContext.RequestServices);
            var thisEntity = await q.Where(x=> x.id.Equals(id)).FirstAsync();

            if (thisEntity == null)
            {
                return NotFound();
            }
            var s = $"{id}/{propName}-{range}-{sort}-{filter}";
            //Response.Headers.Add("Cache-Control", "public,max-age=3600");
            /*CacheTimer d; this wrong code : dataye yek user ro mide be yeki dige 
            if (cache.TryGetValue(s,out d))
            {
                if(d.time+TimeSpan.FromMinutes(5)<DateTime.UtcNow)
                    return d.data;
                else
                    cache.Remove(s);
            }*/
            
            var range2 = range.convertToRange();
            dynamic thisDotPropertyValue = null;
            Type thisDotPropertyType = null;
            var TT = thisEntity.GetType(); //typeof(T) incoorect becuse we have inheritad tables
            MemberInfo memberInfo=null;
            try
            {
                var thisDotProperty = TT.GetProperty(propName);
                memberInfo = thisDotProperty;
                if (thisDotProperty != null)
                {
                    thisDotPropertyType = thisDotProperty.PropertyType;
                    if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {

                        //thisDotPropertyValue = _context.Entry(thisEntity).Col

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
                
               var thisDotProperty = TT.GetMethod(propName);
               memberInfo = thisDotProperty;
                if (thisDotProperty != null)
                {
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
            var thisDotPropertyItemType = thisDotPropertyType.GetGenericArguments()[0];
            if(getNUser()==null && !thisDotPropertyItemType.GetCustomAttributes<PublicClass>().Any())
                return Unauthorized(); 
            var total = Queryable.Count(thisDotPropertyValue);//TODO most add filter
                                                              //thisDotPropertyValue = pp[propName](this, thisEntity);

            thisDotPropertyValue = EntityFrameworkExtensions.addSecurityFilter(thisDotPropertyValue, HttpContext.RequestServices);

            var sr = sort.convertToSort();
            thisDotPropertyValue = EntityFrameworkExtensions.addSort(thisDotPropertyValue, sr);

            thisDotPropertyValue = EntityFrameworkExtensions.addPagination(thisDotPropertyValue, range2, sr, filter);
            

            var x = await EntityFrameworkQueryableExtensions.ToListAsync(thisDotPropertyValue);


            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {thisDotPropertyItemType.Name}  {range2.Item1}-{range2.Item2}/{total}");
            var res=JToken.FromObject(x, serializer);
            //cache[s] = new() { data = res, time = DateTime.UtcNow };
            //_context.ChangeTracker.Clear();
            //System.GC.Collect();
            return res;



        }


    }




}


