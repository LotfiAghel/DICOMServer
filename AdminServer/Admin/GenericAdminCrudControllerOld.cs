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
using System.Linq.Expressions;
using AdminServer.Services;
using WebApplication.Controllers;
using Tools;
using Models;
using EnglishToefl.Data;
using AdminPanel;
using ViewGeneratorBase;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections;
using EnglishToefl.Helper;
using Microsoft.IdentityModel.Tokens;
using NLog.Filters;

namespace AdminPanelOld
{

    


    public class GenericClientControllerRouteConvention : IControllerModelConvention
    {
        public string url;
        public Type type;
        public bool addSwagger = false;
        private string v;
        private List<Type> tmp;

        public GenericClientControllerRouteConvention(Type type, string url, List<Type> tmp, bool addSwagger = false)
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
                string surl = "";
                /*var customNameAttribute = genericType.GetCustomAttribute<GeneratedControllerAttribute>();
                if (customNameAttribute == null)
                    return;
                
                if (customNameAttribute.Route != null)
                {
                    surl = url + customNameAttribute.Route;
                }
                else*/
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
        Type type;
        List<Type> types;
        public GenericTypeControllerFeatureProvider(Type type, List<Type> types = null)
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
                    var candidates = currentAssembly.GetExportedTypes().Where(x => x.IsAssignableTo(typeof(IEntity3<int>)) || x.IsAssignableTo(typeof(IEntity3<Guid>)));
                    //var candidates=new List<Type>(){typeof(Dashboard)}

                    foreach (var candidate in candidates)
                    {
                        //Console.WriteLine(candidate.FullName);
                        foreach(var keyType in new Type[] { typeof(int),typeof(string),typeof(Guid)})
                            if (candidate.IsAssignableTo(typeof(IEntity3<>).MakeGenericType(keyType)))
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
          where T :    class, Models.IIdMapper<TKEY>
         where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
    {
        private readonly ApplicationDbContext _context;
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
        

        
        static JsonSerializer serializer = JsonSerializer.Create(settings);
        IAdminUserService userService;
        static GenericAdminCrudController(){
            serializer.Converters.Add(new RialConverter());

            serializer.Converters.Add(new ForeignKeyConverter());
            serializer.Converters.Add(new ForeignKey2Converter<string>());
            serializer.Converters.Add(new ForeignKey2Converter<int>());
        }

        public GenericAdminCrudController(ApplicationDbContext context, IAdminUserService userService)
        {
            _context = context;
            _db = context.Set<T>();
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
            var range2 = range.convertToRange();
            var total = q.Count<T>();//TODO must add filter
            var sr = sort.convertToSort();
            q = EntityFrameworkExtensions.addPagination<T>(q, range2, sr, filter);

            var x = await q.ToListAsync();


            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {typeof(T).Name}  {range2.Item1}-{range2.Item2}/{total}");
            return JToken.FromObject(x, serializer);
        }



        [HttpPost("getAll")]
        public async Task<ActionResult<JToken>> GetEntitys2([FromBody] IQueryContainer<T> inp)
        {
            if (!await checkPermission<SelectAccess>()) return Forbid();
            var q = inp != null ? inp.query.run(_db) : _db;// _db.Where(x => true);
            //var q2= from u in q select new {u.id};
            
            q = EntityFrameworkExtensions.addSort<T>(q,  null);
            var x = (await q.ToListAsync());//.ConvertAll(x=>new ForeignKey<T>(x.id));
            var total = x.Count();

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $"{total}");
            return JToken.FromObject(x, serializer);
            //return x;
        }

        [HttpPost("{id}/runAction")]
        public async Task<ActionResult<JToken>> runAction(TKEY id, [FromBody] IAction<T> inp)
        {
            if (!await checkPermission<UpdateAccess>()) return Forbid(); //TODO check action Access
            var en = await _db.FindAsync(id);
            await inp.run(en, HttpContext.RequestServices);
            _context.Entry(en).State = EntityState.Modified;

            
            await _context.SaveChangesAsync();
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





        [HttpGet("{id}")]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id)
        {
            if (!await checkPermission<ViewAccess>()) return Forbid();

            var entity = await _db.FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            return JToken.FromObject(entity, serializer);
        }

        [HttpGet("{id}/{propName}")]
        [IgnoreDocAttribute]
        public async Task<ActionResult<JToken>> GetEntity(TKEY id, string propName, string range, string sort, string filter)
        {
            if (!await checkPermission<ViewAccess>()) return Forbid(); //TODO check select access detail Entity
            var thisEntity = await _db.FindAsync(id);

            if (thisEntity == null)
            {
                return NotFound();
            }

            var range2 = range.convertToRange();
            dynamic thisDotPropertyValue=null;
            Type thisDotPropertyType=null;
            try
            {
                var thisDotProperty = typeof(T).GetProperty(propName);
                thisDotPropertyType = thisDotProperty.PropertyType;
                if (thisDotPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {


                    
                    thisDotPropertyValue =_context.Entry(thisEntity)
                        .Collection(propName)
                        .Query();


                   
                }

            }
            catch
            {

            }
            try{
                var thisDotProperty = typeof(T).GetMethod(propName);
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
            catch{

            }
            var thisDotPropertyItemType = thisDotPropertyType.GetGenericArguments()[0];
            var total = Queryable.Count(thisDotPropertyValue);//TODO most add filter
            //thisDotPropertyValue = pp[propName](this, thisEntity);
            var sr = sort.convertToSort();
            thisDotPropertyValue = EntityFrameworkExtensions.addPagination(thisDotPropertyValue, range2, sr, filter);

            var x = await EntityFrameworkQueryableExtensions.ToListAsync(thisDotPropertyValue);
            

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            Response.Headers.Add("Content-Range", $" {thisDotPropertyItemType.Name}  {range2.Item1}-{range2.Item2}/{total}");
            return JToken.FromObject(x, serializer);

            

        }




        


        [HttpPut("{id}")]
        public async Task<ActionResult<JToken>> PutEntity(TKEY id, [FromBody] JObject jentity)
        {
            if (!await checkPermission<UpdateAccess>()) return Forbid();

            bool adminWriteBan = typeof(T).GetCustomAttributes(typeof(AdminWriteBan), true).ToList().GetFirst<object, Models.AdminWriteBan>() != null;
            if (adminWriteBan)
                return NoContent();
            var entity = await _db.FindAsync(id);
            if (entity == null)
                return NotFound();
            var entity2 = jentity.ToObject<ObjectContainer<T>>(serializer).data;
            entity2.id = entity.id;
            _context.Entry(entity).CurrentValues.SetValues(entity2);
            _context.Entry(entity).State = EntityState.Modified;

            //JsonConvert.PopulateObject(jentity.ToString(), entity, settings2);


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
                return JToken.FromObject(new { data = entity }, serializer)["data"];
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
            
            try
            {
                var user = await userService.check(Request);
                if(user==null)
                    return false;
                var xx = typeof(T).GetCustomAttributes<T2>().ToArray();
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

        [HttpPost]
        public async Task<ActionResult<JToken>> PostEntity([FromBody] JToken jentity)
        {
            if (!await checkPermission<InsertAccess>())
                return Forbid();




            var entity = jentity.ToObject<T>(serializer);

            //WebApplication.Helpers.AdminUserChecker.check(HttpContext.Request);
            //var e=data.ToObject<T>();
            _db.Add(entity);
            
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get" + typeof(T).Name, new { id = entity.id }, JToken.FromObject(entity, serializer));
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
