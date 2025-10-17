using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using Newtonsoft.Json;
using Tools;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Models.Notifications;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Models.DataExtract;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Parbad.Storage.Abstractions;

namespace  Data
{
}
namespace Data.Data
{
    public class DBContext : DbContext, IAssetManager, IStorage
    {
        
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            registerDbSet();
        }

        
            
        //[DbFunction("pg_column_size", "public",IsBuiltIn = true)]
        public static int PgColumnSize(ICollection column)
        {
            throw new NotSupportedException("This method is for use with EF Core LINQ queries only.");
        }
        
        
        
      
        
        private Dictionary<Type, object> objectsHistory = new Dictionary<Type, object>();
        //private Dictionary<string, object> objects2 = new Dictionary<string, object>();

        public DbSet<AdminUser> AdminUsers { get; set; }


        
        public DbSet<EntityHistory<Guid>> history { get; set; }
        public DbSet<EntityHistory<int>> history2 { get; set; }
        public DbSet<Models.Admin.AdminApiCall> adminApiCalls { get; set; }
        
        
        public DbSet<User> users { get; set; }
        public DbSet<UserExaminer> userExaminers { get; set; }

        
        public DbSet<UserMigrateData> usersMigrateData { get; set; }


        
        
        public DbSet<UserExtraData> userExtraDatas { get; set; }

        public DbSet<Profile> profiles { get; set; }

        //public DbSet<Market> markets { get; set; }
        public DbSet<VerificationCode> verificationCodes { get; set; }


        public static DBContext instance;


        
        public DbSet<News> news { get; set; }
        public DbSet<UserInboxItem> userInboxItems { get; set; }


        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<SendSmsHistory> SendSmsHistories { get; set; }

        public DbSet<Payment> payments { get; set; }
        public DbSet<Transaction> transactions { get; set; }
        
        public DbSet<DataSourceJsonPair> dataSourceJsonPairs { get; set; }
        public DbSet<DataSourceJsonConvertor> dataSourceJsonConvertors { get; set; }

        public IQueryable<Payment> Payments
        {
            get => payments;
        }

        public IQueryable<Transaction> Transactions
        {
            get => transactions;
        }

        public void registerDbSet()
        {
            instance = this;

            objectsHistory[typeof(Guid)] = history;
            objectsHistory[typeof(int)] = history2;
        }

        public DbSet<T> dbSet<T>() where T : class
        {
            /*if (typeof(T) == typeof(Student))
            {
                return (objects[typeof(T)] = Students) as DbSet<T>;
            }/**/
            return Set<T>();
        }

        public DbSet<EntityHistory<T>> dbSetHistory<T>() where T : IEquatable<T>, IComparable<T>, IComparable
        {
            return objectsHistory[typeof(T)] as DbSet<EntityHistory<T>>;
        }


        internal class FArrayComparer<T, TKEY> : ValueComparer<List<ForeignKey2<T, TKEY>>>
            where T : class, Models.IIdMapper<TKEY>
            where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {
            public FArrayComparer()
                : base((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Value.GetHashCode())),
                    c => c.ToList())
            {
            }
        }

        internal class FArrayComparerInt<T> : ValueComparer<List<ForeignKey2<T, int>>>
            where T : class, Models.IIdMapper<int>

        {
            public FArrayComparerInt()
                : base((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(default(int), (a, v) => HashCode.Combine(a, (int)v.Value)),
                    c => c.ToList())
            {
            }
        }

        internal class FArrayComparerGuid<T> : ValueComparer<List<ForeignKey2<T, Guid>>>
            where T : class, Models.IIdMapper<Guid>

        {
            public FArrayComparerGuid()
                : base((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Value.GetHashCode())),
                    c => c.ToList())
            {
            }
        }


        internal class AdminRoleArrayComparer : ValueComparer<List<AdminUserRole>>
        {
            public AdminRoleArrayComparer()
                : base((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, (int)v)),
                    c => c.ToList())
            {
            }
        }

        internal class AdminRoleComparer : ValueComparer<List<AdminUserRole>>
        {
            public AdminRoleComparer()
                : base((c1, c2) => c1.Equals(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, (int)v)),
                    c => c.ToList())
            {
            }
        }

        internal class AdminRoleConvertor : ValueConverter<AdminUserRole, int>
        {
            public AdminRoleConvertor() :
                base(v => (int)v,
                    v => (AdminUserRole)v,
                    null)
            {
            }
        }

        internal class AdminRoleArrayConvertor : ValueConverter<List<AdminUserRole>, int[]>
        {
            public AdminRoleArrayConvertor() :
                base(v => v.Select(x => (int)x).ToArray(),
                    v => v.Select(x => (AdminUserRole)x).ToList(),
                    null)
            {
            }
        }

        /*internal class FooModelComparer<T, TKEY> : ValueComparer<ForeignKey2<T, TKEY>>
                      where T : class, Models.IIdMapper<TKEY>
     where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {
            public FooModelComparer()
                : base((c1, c2) => c1.Value.Equals(c2.Value),
                        c => c.Value,
                        c => c.ToList())
            { }
        }*/
        public class FooModelCoverter<T, TKEY> : ValueConverter<ForeignKey2<T, TKEY>, TKEY>
            where T : class, Models.IIdMapper<TKEY>
            where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {
            public FooModelCoverter() :
                base(v => v.Value,
                    v => new ForeignKey2<T, TKEY>(v),
                    null)
            {
            }
        }

        public class JsonConvertor<T> : ValueConverter<T, string>


        {
            public JsonConvertor() :
                base(v => JToken.FromObject(v).ToString(),
                    v => JToken.Parse(v).ToObject<T>(),
                    null)
            {
            }
        }

        /*protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<List<AdminUserRole>>().HaveConversion<AdminRoleArrayConvertor, AdminRoleArrayComparer>();
            configurationBuilder.Properties<AdminUserRole>().HaveConversion<AdminRoleConvertor, AdminRoleComparer>();
            //configurationBuilder.Properties<AdminUserRole>().Have<AdminRoleConvertor, FooModelComparer>();
        }*/
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            /*configurationBuilder
                .Properties<List<ForeignKey2<LeitnerWordTag, int>>>()
                .HaveConversion<FooModelCoverter<LeitnerWordTag, int>>();*/
        }

        public static void addForType(ModelBuilder modelBuilder, Type type)
        {
            {
                var props = type.GetProperties().Where(x =>
                    x.GetCustomAttributes<ColumnAttribute>().Where(x => x.TypeName == "jsonb[]").Any());
                foreach (var prop in props)
                {
                    var convertorType = typeof(JsonConvertor<>).MakeGenericType(new Type[]
                        { prop.PropertyType.GenericTypeArguments[0] });
                    modelBuilder.Entity(type)
                        .PrimitiveCollection(prop.Name).ElementType().HasConversion(convertorType);
                }
            }
            {
                var props = type.GetProperties().Where(x =>
                    x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    && x.PropertyType.GenericTypeArguments[0].IsGenericType
                    && x.PropertyType.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(ForeignKey2<,>)
                );
                foreach (var prop in props)
                {
                    var convertorType =
                        typeof(FooModelCoverter<,>).MakeGenericType(prop.PropertyType.GenericTypeArguments[0]
                            .GenericTypeArguments);
                    modelBuilder.Entity(type)
                        .PrimitiveCollection(prop.Name).ElementType().HasConversion(convertorType);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            if (false)
            {
                var b = System.Diagnostics.Debugger.Launch();
                Console.WriteLine($"Debugger is attached! {b}");
            }

            modelBuilder.HasDbFunction(() => PgColumnSize(default))
                .HasName("pg_column_size").IsBuiltIn().HasParameter("column").Metadata.TypeMapping = new NpgsqlJsonTypeMapping("jsonb",typeof(ICollection));
            
            
            
            
           

           

            object value = modelBuilder.HasPostgresEnum<AdminUserRole>();

            
            //TODO this code not handle inhertnce tables
            var types = typeof(DBContext).GetProperties()
                .Where(x => x.PropertyType.IsGenericType &&
                            x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(x => x.PropertyType.GenericTypeArguments[0]).ToList();


            foreach (var type in types)
            {
                addForType(modelBuilder, type);
            }


          


            modelBuilder.Entity<AdminUser>()
                .PrimitiveCollection(e => e.roles)
                .ElementType()
                .HasConversion(typeof(AdminRoleConvertor));


            //modelBuilder.FinalizeModel();
        }

        public static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            SerializationBinder = TypeNameSerializationBinder.gloabl,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        static DBContext()
        {
            settings.Converters.Add(new ForeignKeyConverter());
            settings.Converters.Add(new RialConverter());
            


            
        }


        public static NpgsqlDataSource createDataSource()
        {
            JsonConvert.DefaultSettings = () => settings;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var Database = Environment.GetEnvironmentVariable("db_name") ?? "test_helper";
            Console.WriteLine(Database);
            var Username = Environment.GetEnvironmentVariable("db_user") ?? "postgres";
            var Password = Environment.GetEnvironmentVariable("db_pass") ?? "DICOMPASS";
            var Host = Environment.GetEnvironmentVariable("db_host") ?? "localhost";
            var Port = int.Parse(Environment.GetEnvironmentVariable("db_port") ?? "5432");


            var tt = $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";


            //Console.WriteLine(t1);

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(tt);
            dataSourceBuilder.UseJsonNet(settings, null, null);
            //dataSourceBuilder.UseJsonNet()
            //dataSourceBuilder
            //.UseLoggerFactory(loggerFactory) // Configure ADO.NET logging
            //.UsePeriodicPasswordProvider(); // Automatically rotate the password periodically
            dataSourceBuilder.MapComposite<Rial>("Integer");
            dataSourceBuilder.MapComposite<Dictionary<int,object>>("jsonb");
            dataSourceBuilder.MapComposite<IForeignKey2<int>>("Integer");
            //dataSourceBuilder.MapComposite<IForeignKey2<Guid>>("uuid");
            dataSourceBuilder.MapEnum<AdminUserRole>();

            //dataSourceBuilder.MapComposite<ForeignKey2<LeitnerWordTag ,Guid>>("uuid");


            return dataSourceBuilder.Build();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(x =>
                x.Ignore(CoreEventId
                    .ManyServiceProvidersCreatedWarning)); //TODO this ignore issue warning you shuld remove this carefully  if you remove this after many query on data base you will see excption
            


            //var dataSource2 = createDataSource();
            //optionsBuilder.UseNpgsql(dataSource2);
            optionsBuilder.UseSnakeCaseNamingConvention(); //this config work on moigration and runing
            //optionsBuilder.EnableServiceProviderCaching(true);
        }

        public IEntityManagerW<T, TKEY> getManager<T, TKEY>()
            where T : class, Models.IIdMapper<TKEY>
            where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> getDbSet<T>() where T : class
        {
            return Set<T>();
        }

        public T2 getDbSet2<T2>() where T2 : class
        {
            throw new NotImplementedException();
        }

        public Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = new CancellationToken())
        {
            payment.Id = 0; //GenerateNewPaymentId();

            payments.Add(payment);
            return SaveChangesAsync(cancellationToken);

            //return Task.CompletedTask;
        }

        public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = new CancellationToken())
        {
            Entry(payment).State = EntityState.Modified;
            return SaveChangesAsync(cancellationToken);
        }

        public Task DeletePaymentAsync(Payment payment, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task CreateTransactionAsync(Transaction transaction,
            CancellationToken cancellationToken = new CancellationToken())
        {
            transaction.Id = 0; //GenerateNewPaymentId();

            transactions.Add(transaction);
            return SaveChangesAsync(cancellationToken);
        }

        public Task UpdateTransactionAsync(Transaction transaction,
            CancellationToken cancellationToken = new CancellationToken())
        {
            Entry(transaction).State = EntityState.Modified;
            return SaveChangesAsync(cancellationToken);
        }

        public Task DeleteTransactionAsync(Transaction transaction,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}