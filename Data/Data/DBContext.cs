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
using Models.Books;
using Models.Management;
using Models.Exams;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Models.DataExtract;
using Models.Dictionary;
using Models.Target;
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
        
        [DbFunction("jsonb_object_keys", "public", IsBuiltIn = true)]
        public static IEnumerable<int> JsonbObjectKeys(Dictionary<int, Models.UserSectionData.QuestionResponseSummary> column)
        {
            throw new NotSupportedException("This method is for use with EF Core LINQ queries only.");
        }
        
        [DbFunction("jsonb_key_count", "public")]
        public static int JsonbKeyCount(Dictionary<int, Models.UserSectionData.QuestionResponseSummary>  column)
        {
            throw new NotSupportedException("This method is for use with EF Core LINQ queries only.");
            /*CREATE OR REPLACE FUNCTION public.jsonb_key_count(j jsonb)
               RETURNS integer AS $$
               BEGIN
                   RETURN (SELECT COUNT(*) FROM jsonb_object_keys(j));
               END;
               $$ LANGUAGE plpgsql IMMUTABLE;*/
        }
        
      
        
        private Dictionary<Type, object> objectsHistory = new Dictionary<Type, object>();
        //private Dictionary<string, object> objects2 = new Dictionary<string, object>();

        public DbSet<AdminUser> AdminUsers { get; set; }


        public DbSet<Models.Shadowing.VideoContent> VideoContents { get; set; }
        public DbSet<EntityHistory<Guid>> history { get; set; }
        public DbSet<EntityHistory<int>> history2 { get; set; }
        public DbSet<Models.Admin.AdminApiCall> adminApiCalls { get; set; }
        
        
        public DbSet<User> users { get; set; }
        public DbSet<UserExaminer> userExaminers { get; set; }

        public DbSet<PhysicalExam> physicalExams { get; set; }
        public DbSet<PhysicalExamClientRel> physicalExamClientRels { get; set; }

        public DbSet<UserMigrateData> usersMigrateData { get; set; }


        public DbSet<UserLog> loggs { get; set; }
        
        public DbSet<AppDownloadFile> appDownloadFiles { get; set; }

        
        public DbSet<UserExtraData> userExtraDatas { get; set; }

        public DbSet<Profile> profiles { get; set; }

        //public DbSet<Market> markets { get; set; }
        public DbSet<VerificationCode> verificationCodes { get; set; }


        public static DBContext instance;


        public DbSet<Advertisment> Advertisments { get; set; }
        public DbSet<Analysis> Analysises { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Company> Companies { get; set; }

        public DbSet<DeviceSessions> DeviceSessions { get; set; }

        public DbSet<Exam> Exams { get; set; }

        //public DbSet<ExamPart> ExamParts { get; set; }
        public DbSet<ImportantWords> ImportantWords { get; set; }

        public DbSet<LeitnerWordPack> leitnerWordPacks { get; set; }
        public DbSet<LeitnerWordPackRel> leitnerWordPackRels { get; set; }

        public DbSet<UserLeitnerWord> userLeitnerWords { get; set; }


        public DbSet<LeitnerWord> LeitnerWords { get; set; }
        public DbSet<UserLeitnerCard> LeitnerCardUsers { get; set; }
        public DbSet<UserLeitnerCardCheckPoint> LeitnerCardUserCheckPoints { get; set; }
        public DbSet<UserLeitnerAction> LeitnerUserActions { get; set; }
        public DbSet<LeitnerWordTag> leitnerWordTags { get; set; }

        public DbSet<ApplicationUser> applicationUsers { get; set; }


        public DbSet<News> news { get; set; }
        public DbSet<UserInboxItem> userInboxItems { get; set; }


        public DbSet<Province> Provinces { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionPack> questionPacks { get; set; }
        
        
        public DbSet<ExamStatistics> examStatistics { get; set; }
        public DbSet<QuestionStatistics> questionStatistics { get; set; }
        public DbSet<SectionStatistics> sectionStatistics { get; set; }
        
        
        public DbSet<Tracker> trackers { get; set; }


        public DbSet<WritingAvatar> WritingAvatars { get; set; }

        //public DbSet<QuestionOptions> QuestionOptions { get; set; }
        //public DbSet<QuestionTableOptions> questionTableOptions { get; set; }


        public DbSet<Models.TextTools.UserNote> userNotes { get; set; }
        public DbSet<Models.TextTools.UserNoteTag> userNoteTags { get; set; }
        
        public DbSet<Response> Responses { get; set; }
        public DbSet<AResponseAdjust> responseAdjusts { get; set; }
        public DbSet<KeyPointOfSections> keyPointOfSections { get; set; }

        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionUserProgress> SectionUserProgresses { get; set; }
        public DbSet<SelfDictionary> SelfDictionaries { get; set; }
        public DbSet<Service> Services { get; set; }

        public DbSet<ServiceGroup> ServiceGroups { get; set; }

        public DbSet<ServiceUseLog> ServiceUseLogs { get; set; }

        public DbSet<ServiceBuy> ServiceBuys { get; set; }
        
        
        
        public DbSet<PublicDiscount> PublicDiscounts { get; set; }
        public DbSet<PublicDiscountUse> PublicDiscountUses { get; set; }

        //public DbSet<SyncQueue> SyncQueue { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        public DbSet<TicketingCategory> TicketingCategory { get; set; }
        public DbSet<Ticketing> Ticketing { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<SendSmsHistory> SendSmsHistories { get; set; }

        public DbSet<SectionType> SectionTypes { get; set; }

        public DbSet<SectionDirection> SectionDirections { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<ClassSession> ClassSessions { get; set; }

        public DbSet<SectionPart> SectionParts { get; set; }
        public DbSet<ParagraphContainer> ParagraphContainers { get; set; }

        public DbSet<QuestionSample> QuestionSamples { get; set; }

        //public DbSet<ClientSyncRequestError> Errors { get; set; }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookPage> BookPages { get; set; }
        public DbSet<UserBookmark> UserBookmarks { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<SectionPartSubject> SectionPartSubjects { get; set; }
        public DbSet<SectionSubject> SectionSubjects { get; set; }

        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<ExamPartSession> ExamPartSessions { get; set; }
        public DbSet<ExamPartSessionLogs> examPartSessionLogs { get; set; }

        public DbSet<UserSectionData> userSectionDatas { get; set; }

        public DbSet<UserExamPartData> userExamPartDatas { get; set; }
        public DbSet<UserSubjectData> UserSubjectDatas { get; set; }


        public DbSet<Notice> notices { get; set; }


        public DbSet<Models.JobTime.Project> JobProjects { get; set; }
        public DbSet<Models.JobTime.JobTask> JobTasks { get; set; }
        public DbSet<Models.JobTime.GitCommit> GitCommits { get; set; }
        public DbSet<Models.JobTime.TimeSpanJob> TimeSpanJobs { get; set; }


        public DbSet<Models.Interview.User> InterViewUser { get; set; }
        public DbSet<Models.Interview.Task> InterViewTask { get; set; }
        public DbSet<Models.Interview.TaskLog> InterViewTaskStart { get; set; }
        public DbSet<Models.Interview.Response> InterViewResponse { get; set; }


        public DbSet<Models.AiResponse.Content> aiResponseContents { get; set; }
        public DbSet<Models.AiResponse.Report> aiResponseReport { get; set; }


        public DbSet<CachedDictionaryResult> DictionaryResults { get; set; }
        public DbSet<UserRamCacheData> UserRamCacheDatas { get; set; }


        public DbSet<StudentTask> studentTasks { get; set; }
        public DbSet<StudentTaskHistory> studentTaskHistories { get; set; }

        public DbSet<MockScore> mockScores { get; set; }
        public DbSet<RealScoreTarget> realScoreTargets { get; set; }


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
            
            
            modelBuilder.HasDbFunction(() => JsonbObjectKeys(default))
                .HasName("jsonb_object_keys").IsBuiltIn().HasParameter("column").Metadata.TypeMapping = new NpgsqlJsonTypeMapping("jsonb",typeof(ICollection));
            
            modelBuilder.HasDbFunction(() => JsonbKeyCount(default))
                .HasName("jsonb_key_count").IsBuiltIn().HasParameter("column").Metadata.TypeMapping = new NpgsqlJsonTypeMapping("jsonb",typeof(ICollection));

            
            modelBuilder.Entity<Exam>()
                .Property(f => f.id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Company>()
                .Property(f => f.id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<LeitnerWordPack>()
                .HasMany(e => e.cards2)
                .WithMany(e => e.packs)
                .UsingEntity<LeitnerWordPackRel>();


            object value = modelBuilder.HasPostgresEnum<AdminUserRole>();

            modelBuilder.Entity<Question>().ToTable("questions")
                .HasDiscriminator<string>("type")
                .HasValue<QuestionOptinalSingle>(nameof(QuestionOptinalSingle))
                .HasValue<QuestionOptinalMultiOrdered>(nameof(QuestionOptinalMultiOrdered))
                .HasValue<BlankSpaceQuestion>(nameof(BlankSpaceQuestion))
                .HasValue<MultiOptionConstPoint>(nameof(MultiOptionConstPoint))
                .HasValue<QuestionOptinalMulti>(nameof(QuestionOptinalMulti))
                .HasValue<MultimediaQuestion>(nameof(MultimediaQuestion))
                .HasValue<TabledChoosed>(nameof(TabledChoosed))
                .HasValue<QuestionTrueFalseOption>(nameof(QuestionTrueFalseOption))
                .HasValue<StringQuestion>(nameof(StringQuestion))
                .HasValue<WordQuestion>(nameof(WordQuestion))
                .HasValue<QuantWhichIsGreaterQuestion>(nameof(QuantWhichIsGreaterQuestion))
                //.HasValue<QuestionOptinal>(nameof(QuestionOptinal))
                //.HasValue<QuestionOptinalMultiOrdered>(nameof(QuestionOptinalMultiOrdered))
                ;


            modelBuilder.Entity<AResponseAdjust>().ToTable("response_adjusts")
                .HasDiscriminator<string>("type")
                .HasValue<SpeakingAdjustment>(nameof(SpeakingAdjustment))
                .HasValue<ResponseAdjust>(nameof(ResponseAdjust))
                .HasValue<IeltsWritingAdjust>(nameof(IeltsWritingAdjust))
                .HasValue<IeltsSpeakingAdjustment>(nameof(IeltsSpeakingAdjustment))
                .HasValue<TOEFLWritingAdjust>(nameof(TOEFLWritingAdjust));
            
                
            


            modelBuilder.Entity<Response>().ToTable("responses")
                .HasDiscriminator<string>("type")
                .HasValue<QuestionTrueFalseOptionResponse>(nameof(QuestionTrueFalseOptionResponse))
                .HasValue<SingleOptionResponse>(nameof(SingleOptionResponse))
                .HasValue<QuestionOptinalResponse>(nameof(QuestionOptinalResponse))
                .HasValue<QuestionOptinalOrderedResponse>(nameof(QuestionOptinalOrderedResponse))
                .HasValue<WritingResponse>(nameof(WritingResponse))
                .HasValue<SpeakingResponse>(nameof(SpeakingResponse))
                .HasValue<WordResponse>(nameof(WordResponse))
                .HasValue<QuantWhichIsGreaterResponse>(nameof(QuantWhichIsGreaterResponse));


            //TODO this code not handle inhertnce tables
            var types = typeof(DBContext).GetProperties()
                .Where(x => x.PropertyType.IsGenericType &&
                            x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(x => x.PropertyType.GenericTypeArguments[0]).ToList();


            foreach (var type in types)
            {
                addForType(modelBuilder, type);
            }


            addForType(modelBuilder, typeof(QuestionOptinal));
            addForType(modelBuilder, typeof(QuestionTrueFalseOption));
            addForType(modelBuilder, typeof(QuestionTrueFalseOptionResponse));
            addForType(modelBuilder, typeof(StringQuestion));

           


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
            settings.Converters.Add(new CompanySkillConverter());


            
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