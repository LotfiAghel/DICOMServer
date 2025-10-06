using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Models;
using System.Linq;
using Tools;
using System.Text.RegularExpressions;
using AdminClientViewModels;
using AdminBaseComponenets;
using AdminClient.Components;
using Blazorise.RichTextEdit;
using Blazorise;
using Blazorise.Material;
using EnglishToefl.Models;
using Blazorise.Icons.Material;
using EnglishToefl.Models.Management;
using EnglishToefl.Models.Exams;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;
using AdminMsgs;
using AdminClient.Components.InGrid;
using Blazorise.Extensions;
using ClientMsgs;
using Models.DataExtract;
using Sitko.Blazor.CKEditor;


namespace AdminClient
{


    public class DbManager : Models.IAssetManager
    {
        public Models.IEntityManagerW<T, TKEY> getManager<T, TKEY>()
             where T : class, Models.IIdMapper<TKEY>
            where TKEY : IEquatable<TKEY>, IComparable<TKEY>, IComparable
        {
            var b = typeof(NewEntityService<,>).MakeGenericType(new Type[] { typeof(T), typeof(TKEY) }).GetConstructor(new Type[] { }).Invoke(new object[] { });
            return b as Models.IEntityManagerW<T, TKEY>;
        }
        public IQueryable<T> getDbSet<T>() where T : class
        {
            return null;
        }
        public T2 getDbSet2<T2>() where T2 : class
        {
            return null;
        }
    }

    public class Program
    {
        public static Dictionary<string, Dictionary<string, object>> parms = new Dictionary<string, Dictionary<string, object>>();
        public static Dictionary<string, object> getRouteParm(string url)
        {
            Console.WriteLine("get0s url data " + url);
            if (!parms.ContainsKey(url))
            {
                Console.WriteLine("create " + url);
                parms[url] = new Dictionary<string, object>();
            }
            return parms[url];
        }
        public static List<ATreeNode> enitits = new List<ATreeNode>();

        public static List<Type> docEntity = new List<Type>();
        public static List<object> aa = new List<object>();
        public static List<Type> nenitits = new List<Type>();
        //public static Dictionary<Type, IEntityService00> dp = new Dictionary<Type, IEntityService00>();

        /*public static Dictionary<Type, Func<object>> Program0.defultCunstroctor = new Dictionary<Type, Func<object>>();
        public static Dictionary<Type, Func<List<Attribute>, ComponentBase>> Program0.defultRenderer = new Dictionary<Type, Func<List<Attribute>, ComponentBase>>();
        public static Dictionary<Type, Func<Type, List<Attribute>, Type>> Program0.defultRenderer2 = new Dictionary<Type, Func<Type, List<Attribute>, Type>>();


        public static Dictionary<Type, Func<List<Attribute>, ComponentBase>> Program0.formRenderer = new Dictionary<Type, Func<List<Attribute>, ComponentBase>>();


        public static Dictionary<Type,bool> Program0.inPropRender = new Dictionary<Type, bool>();
        public static Dictionary<Type, Func<Type, List<Attribute>, Type>> Program0.formRenderer2 = new Dictionary<Type, Func<Type, List<Attribute>, Type>>();
*/

        public enum ViewType
        {
            NONE = 0,
            SMALL_VIEW = 1,
            VIEW = 2,
            FORM = 3,
        }
        /* public static RenderFragment CreateDynamicComponent0(object thiz, Type gt, object vv, Action<object> v = null, List<Attribute> Attributes = null) => builder =>
              {


                  Console.WriteLine(" CreateDynamicComponent0 " + gt.Name);

                  builder.OpenComponent(0, gt);

                  if (v != null)
                  {
                      var callback = EventCallback.Factory.Create<object>(thiz, v);
                      builder.AddAttribute(1, "changeRefrence", callback);
                  }
                  if (v != null)
                  {
                      builder.AddAttribute(1, "Attributes", Attributes);
                  }

                  builder.AddAttribute(1, "value", vv);

                  builder.CloseComponent();
              };
        /**/

    

        public static void GenerateWidgetsFunctions()
        {

            Program0.defultRenderer[typeof(User)] = (prps) =>
            {

                return new UserFSmallView();


            };

            /*Program0.defultRenderer[typeof(IHaveName)] = (prps) =>
            {

                return new IHaveNameSmall();


            };*/







        }


        public static void GenerateForm()
        {
            





            /*Program0.formRenderer[typeof(Models.User)] = (prps) =>
            {


                return new AdminClient.Components.User.Edit();
            };/**/




















        }







        public static void addApiRegex(string url, Type t)
        {
            var rr = "";
            int b = 0;
            foreach (var c in url)
            {
                if (c == '{')
                    ++b;
                if (c == '}')
                {
                    --b;
                    if (b == 0)
                    {
                        rr += "[0-9]+";
                    }
                    continue;
                }
                if (b == 0)
                {
                    rr += c;
                }
            }

            Regex rgx = new Regex(rr);
            Program0.apis2[rgx] = t;

        }
        public static Type getFuncType(string url)
        {

            if (Program0.apis.ContainsKey(url))
                return Program0.apis[url];
            foreach (var e in Program0.apis2)
            {
                System.Text.RegularExpressions.Match match = e.Key.Match(url);
                if (match.Success)
                    return e.Value;
            }
            return null;

            //return typeof(System.Func<AdminMsgs.TimeRangeRequest,AdminMsgs.MaliCoach>);
        }
        public static async Task<IAdminUser> GetUser()
        {
            if (Program0.user != null)
                return Program0.user;
            var res = await ClTool.WebClient.webClient.fetch<object, AdminMsg.LoginResponse<AdminUser>>("adminUser/getUser",
            HttpMethod.Get, null);
            Program0.user = res.user;
            return Program0.user;
        }
        public static void AddMenu(ATreeNode a, params AdminUserRole[] l)
        {
            if (!Program0.user.roles.Intersect(l).IsNullOrEmpty())
                enitits.Add(a);

        }
        public static List<ATreeNode> ChangeMenu(IAdminUser user)
        {
            
            Program0.apis["adminuser/createMock"] = typeof(FuncV<CreateMockUsers,BooleanResponse>);
            Program0.apis["adminuser/getUsers"] = typeof(FuncV<GetUsers,GetUsersResponse>);
            
            Program0.apis["Statistics/serviceBuyInTime"] = typeof(FuncV<ServiceBuyReq,ServiceBuyResponse>);
            Program0.apis["Statistics/uniqUserPerLearn"] = typeof(FuncV<ServiceBuyReq,UniqUserPerLearnResponse>);
            Program0.apis["Statistics/revise"] = typeof(FuncV<ServiceBuyReq,STReviseResponse>);
            Program0.apis["Statistics/friendShip"] = typeof(FuncV<ServiceBuyReq,STFrResponse>);
            
            Program0.apis["adminuser/addUserWithEmail"] = typeof(FuncV<AddUserWithEmail,ObjectContainer<User>>);
            
            Program0.apis["adminuser/findWithTracker"] = typeof(FuncV<TSerach, BooleanResponse>);
            Program0.apis2[new Regex(@"adminuser/migrateExam/[0-9]")] = typeof(Func<ExamMigrateResult>);
            Program0.apis2[new Regex(@"adminuser/migrateExam2/[0-9]")] = typeof(Func<ExamMigrateResult>);
            Program0.apis2[new Regex(@"adminuser/migrateAll/[0-9]")] = typeof(Func<ExamMigrateResult>);

            Program0.apis2[new Regex(@"adminuser/getSessions/[a-z0-9\-]")] = typeof(Func<GetSessionsResponse>);


            Program0.formRenderer[typeof(List<SubtitleRow>)] = (prps) =>
            {
                return new SubTitleForm();

            };
            Program0.formRenderer[typeof(SessionData)] = (prps) =>
            {
                return new SessionDataForm();
            };
                
            Program0.RegisterForm<Models.User, AdminClient.Components.UserForm>();
            Program0.RegisterForm<Models.SubtitleRow, AdminClient.Components.SubtitleRowForm>();
            Program0.RegisterForm<Models.Exam, AdminClient.Components.ExamForm>();
            Program0.RegisterForm<Models.LeitnerWordPack, AdminClient.Components.LeitnerWordPackForm>();
            Program0.RegisterForm<Models.WritingResponse, AdminClient.Components.WritingResponseForm>();
            
            
            Program0.RegisterForm<Models.WordQuestion, AdminClient.Components.QuestionForms.TrueFalse.WordQuestionForm>();
            Program0.RegisterForm<Models.QuantWhichIsGreaterQuestion, AdminClient.Components.QuestionForms.TrueFalse.QuantWhichIsGreaterQuestion>();
            Program0.RegisterForm<Models.QuestionTrueFalseOption, AdminClient.Components.QuestionForms.TrueFalse.TrueFalseQuestionForm>();
            Program0.RegisterForm<Models.QuestionOptinalSingle, AdminClient.Components.QuestionForms.TrueFalse.QuestionOptinalSingleForm>();
            Program0.RegisterForm<Models.BlankSpaceQuestion, AdminClient.Components.QuestionForms.TrueFalse.BlankSpaceQuestionForm>();
            Program0.RegisterForm<Models.QuestionOptions, AdminClient.Components.QuestionForms.TrueFalse.QuestionOptionsForm>();
            
            Program0.RegisterForm<Models.QuestionSample, AdminClient.Components.QuestionForms.QuestionSampleForm>();
            
            
            Program0.RegisterForm<Models.QuestionTableOptions, AdminClient.Components.QuestionForms.QuestionOptinalD.QuestionTableOptionsForm>();
            Program0.RegisterForm<Models.QuestionCulmns, AdminClient.Components.QuestionForms.QuestionOptinalD.QuestionCulmnsForm>();
            
            Program0.RegisterForm<Models.TabledChoosed, AdminClient.Components.QuestionForms.QuestionOptinalD.TabledChoosedForm>();
            Program0.RegisterForm<Models.QuestionOptinalMultiOrdered, AdminClient.Components.QuestionForms.QuestionOptinalD.QuestionOptinalMultiOrderedForm>();
            
            Program0.RegisterForm<Models.MultimediaQuestion, AdminClient.Components.QuestionForms.QuestionForm,Models.Question>();
            Program0.RegisterForm<Models.StringQuestion, AdminClient.Components.QuestionForms.QuestionForm,Models.Question>();
            
            Program0.RegisterForm<Models.SpeakingResponse, AdminClient.Components.SpeakingResponseForm>();
            
            Program0.RegisterForm<Models.Exams.SectionPart, AdminClient.Components.SectionPartForm>();
            Program0.RegisterForm<Models.AppDownloadFile, AdminClient.Components.AppDownloadFileForm>();
            Program0.RegisterForm<Models.SectionSubject, AdminClient.Components.SubjectSection>();
            Program0.RegisterForm<Models.PhysicalExam, AdminClient.Components.PhysicalExamForm>();
            
            Program0.RegisterInGird<Models.SubtitleRow, SubTitleRowInGrid>();
            





            Program0.user = user;
            enitits.Clear();

            
                 
            AddMenu(new TreeNode()
                {
                    name = " مدیریت آدمین",
                    children =
                    [
                        new EntityModel { Type = typeof(AdminUser) },
                        new EntityModel { Type = typeof(AppDownloadFile) }

                    ]
                },AdminUserRole.SUPER_USER
            );

            

            AddMenu(new TreeNode()
            {
                name = " مدیریت شرکتهای ماک",
                children =
                [
                    new EntityModel() { Type = typeof(Models.PhysicalExam) },
                    new ApiViewNode()
                    {
                        name = " ساخت یوزر ",
                        url = "adminuser/createMock"
                    },

                    new ApiViewNode()
                    {
                        name = " findWithTracker ",
                        url = "adminuser/findWithTracker"
                    }
                ]
            },AdminUserRole.SUPER_USER
            );
            
            AddMenu(new TreeNode()
                {
                    name = " آمار",
                    children = new List<ATreeNode>() {

                        
                        new ApiViewNode() {
                            name = " کاربراین یونیک  ",
                            url = "adminuser/getUsers"
                        },
                        new ApiViewNode() {
                            name = "نمودار خرید  ",
                            url = "Statistics/serviceBuyInTime"
                        },
                        new ApiViewNode() {
                            name = "کاربران یونیک",
                            url = "Statistics/uniqUserPerLearn"
                        },
                        new ApiViewNode() {
                            name = "تصحیح",
                            url = "Statistics/revise"
                        },
                        new ApiViewNode() {
                            name = "استاد شاگردی",
                            url = "Statistics/friendShip"
                        },
                        
                        
                        
                        new ApiViewNode() {
                            name = " ایجاد کاربر ",
                            url = "adminuser/addUserWithEmail"
                        }
                        
                        
                    
                    }
                },AdminUserRole.SUPER_USER
            );

            AddMenu(new TreeNode()
                {
                    name = " مدیریت تبدیل ها",
                    children = new List<ATreeNode>() {

                        new EntityModel(){ Type= typeof(DataSourceJsonConvertor)},
                        new EntityModel(){ Type= typeof(DataSourceJsonPair)},
                        
                    }
                },AdminUserRole.SUPER_USER
            );

            if (false)
            {
                AddMenu(new TreeNode()
                {
                    name = "Job_tracker",
                    children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.JobTime.Project)},
                        new EntityModel(){ Type= typeof(Models.JobTime.JobTask)},
                        new EntityModel(){ Type= typeof(Models.JobTime.GitCommit)},
                        new EntityModel(){ Type= typeof(Models.JobTime.TimeSpanJob)},

                    }

                }, Models.AdminUserRole.DATA_ENTRY);


                AddMenu(new TreeNode()
                {
                    name = "reqrit",
                    children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.Interview.User)},
                        new EntityModel(){ Type= typeof(Models.Interview.Task)},
                        new EntityModel(){ Type= typeof(Models.Interview.TaskLog)},
                        new EntityModel(){ Type= typeof(Models.Interview.Response)},

                    }

                }, Models.AdminUserRole.DATA_ENTRY);






                AddMenu(new TreeNode()
                {
                    name = "گزارشات مارکنیتگ",
                    children = new List<ATreeNode>(){

                        new ApiViewNode() {
                            name=" مثلا یک ",
                            url = "ST/statistics"
                        },
                        new TreeNode(){
                            name = "tree1 ",
                            children = new List<ATreeNode>(){
                                new ApiViewNode() {
                                    name=" tree2",
                                    url = "ST/getUsers"
                                },

                                new ApiViewNode() {
                                    name="tree4 ",
                                    url = "ST/get9User2"
                                },
                            }
                    }



                    }
                });

            }





            var res = new List<ATreeNode>();

            foreach (var t in typeof(IEntity0).GetSubClasses())
                if (Program0.checkPermission<ViewAccess>(t))
                {
                    res.Add(new EntityModel() { Type = t });
                }
            if (false)
                AddMenu(new TreeNode()
                {
                    name = "genrated-class",
                    children = res
                });

            if(true)
            


            AddMenu(new TreeNode()
            {
                name = "کاربران و اسمس",
                children = new()
                    {
                        new PageViewNode(){ name="جستجوی کاربران" , url="/search/"+typeof(Models.User).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی sms" , url="/search/"+typeof(Models.VerificationCode).GetUrlEncodeName()},
                        new EntityModel(){ Type= typeof( Models.User)},
                        new PageViewNode(){ name="خریدها", url="/search/"+typeof(Models.ServiceBuy).GetUrlEncodeName()},
                        new EntityModel(){ Type= typeof( Models.ServiceBuy)},
                        new EntityModel(){ Type= typeof( Models.UserExtraData)},
                        new EntityModel(){ Type= typeof( Models.UserRamCacheData)}
                        
                    }
            }, Models.AdminUserRole.SUPPORT);


            AddMenu(new TreeNode()
            {
                name = "مدیریت آزمونها",
                children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.Management.Tag)},
                        new EntityModel(){ Type= typeof(Models.Company)},
                        new EntityModel(){ Type= typeof( Models.SectionType)},
                        new EntityModel(){ Type= typeof( Models.SectionDirection)},
                        new EntityModel(){ Type= typeof(Models.Exam)},
                        new EntityModel(){ Type= typeof(Models.Section)},
                        new EntityModel(){ Type= typeof( Models.Exams.SectionPart)},
                        new EntityModel(){ Type= typeof( Models.Subject)},
                        new PageViewNode(){ name="جستجوی مضوع", url="/search/"+typeof(Models.Subject).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی سکشن", url="/search/"+typeof(Models.Section).GetUrlEncodeName()},
                        //new PageViewNode(){ name="search sectionSubject " , url="/search/"+typeof(Models.SectionSubject).GetUrlEncodeName()},
                        new EntityModel(){ Type= typeof( Models.Question)},

                        new EntityModel(){ Type= typeof( Models.AiResponse.Content)},
                        new EntityModel(){ Type= typeof( Models.AiResponse.Report)},

                        new PageViewNode(){ name="جستجوی آزمون", url="/search/"+typeof(Models.Exam).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی سوال", url="/search/"+typeof(Models.Question).GetUrlEncodeName()},

                    }

            }, Models.AdminUserRole.DATA_ENTRY, Models.AdminUserRole.SUPPORT);
            
            
            AddMenu(new TreeNode()
            {
                name = "مدیریت دیکشنری",
                children = new()
                {
                    new EntityModel(){ Type= typeof(Models.Dictionary.CachedDictionaryResult)},
                    new PageViewNode(){ name="جستجوی کلمه", url="/search/"+typeof(Models.Dictionary.CachedDictionaryResult).GetUrlEncodeName()},
                    

                }

            }, Models.AdminUserRole.DATA_ENTRY, Models.AdminUserRole.SUPPORT);
            AddMenu(new TreeNode()
            {
                name = "مدیریت اقلام خریدنی در سرور جدید",
                children = new()
                {
                    new EntityModel(){ Type= typeof(Models.ServiceGroup)},
                    new EntityModel(){ Type= typeof(Models.Service)},
                    new EntityModel(){ Type= typeof(Models.PublicDiscount)},
                    new EntityModel(){ Type= typeof(Models.PublicDiscountUse)},
                    new EntityModel(){ Type= typeof(Models.ServiceBuy)},
                    // Program0.user.roles.Contains(Models.AdminUserRole.SUPER_USER) ? new EntityModel(){ Type= typeof(Models.ServiceBuy):null,
                }

            }, Models.AdminUserRole.DATA_ENTRY, Models.AdminUserRole.SUPPORT);




            AddMenu(new TreeNode()
            {
                name = "فلش کارت و لیتنر ",
                children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.LeitnerWordPack)},
                        new EntityModel(){ Type= typeof(Models.LeitnerWord)},
                        new PageViewNode(){ name="جستوجوس کارتهای لایتنر", url="/search/"+typeof(Models.LeitnerWord).GetUrlEncodeName()},
                    }

            }, Models.AdminUserRole.DATA_ENTRY, Models.AdminUserRole.SUPPORT);



            AddMenu(new TreeNode()
            {
                name = "آزمونها و پاسخهای کاربران",
                children = new()
                    {
                        new EntityModel(){ Type= typeof( Models.ExamPartSession)},
                        new EntityModel(){ Type= typeof( Models.ExamPartSessionLogs)},
                        new EntityModel(){ Type= typeof( Models.Response)},
                        new PageViewNode(){ name="جستجوی جلسات آزمونها", url="/search/"+typeof(Models.ExamSession).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی جلسات مهارتها", url="/search/"+typeof(Models.ExamPartSession).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی پاسخها", url="/search/"+typeof(Models.Response).GetUrlEncodeName()},
                        new PageViewNode(){ name="جستجوی تصحیح‌ها", url="/search/"+typeof(Models.AResponseAdjust).GetUrlEncodeName()},
                    }

            }, Models.AdminUserRole.SUPPORT);
            AddMenu(new TreeNode()
            {
                name = "اخبار و اینباکس",
                children = new()
                    {
                        new EntityModel(){ Type= typeof( Models.Notifications.News)},
                        new EntityModel(){ Type= typeof( Models.Notifications.UserInboxItem)},
                    }

            }, Models.AdminUserRole.DATA_ENTRY, Models.AdminUserRole.SUPPORT);

            
            
            AddMenu(new TreeNode()
                {
                    name = " shadowing",
                    children =
                    [
                        new EntityModel() { Type = typeof(Models.Shadowing.Lesson) },
                        new EntityModel() { Type = typeof(Models.Shadowing.VideoContent) }
                    ]
                },AdminUserRole.SUPER_USER
            );
            
                AddMenu(new TreeNode()
                {
                    name = "Job_tracker",
                    children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.JobTime.Project)},
                        new EntityModel(){ Type= typeof(Models.JobTime.JobTask)},
                        new EntityModel(){ Type= typeof(Models.JobTime.GitCommit)},
                        new EntityModel(){ Type= typeof(Models.JobTime.TimeSpanJob)},

                    }

                }, Models.AdminUserRole.DEVELOPER);


                AddMenu(new TreeNode()
                {
                    name = "reqrit",
                    children = new()
                    {
                        new EntityModel(){ Type= typeof(Models.Interview.User)},
                        new EntityModel(){ Type= typeof(Models.Interview.Task)},
                        new EntityModel(){ Type= typeof(Models.Interview.TaskLog)},
                        new EntityModel(){ Type= typeof(Models.Interview.Response)},

                    }

                }, Models.AdminUserRole.DEVELOPER);






                AddMenu(new TreeNode()
                {
                    name = "گزارشات مارکنیتگ",
                    children = new List<ATreeNode>(){

                        new ApiViewNode() {
                            name=" مثلا یک ",
                            url = "ST/statistics"
                        },
                        new TreeNode(){
                            name = "tree1 ",
                            children = new List<ATreeNode>(){
                                new ApiViewNode() {
                                    name=" tree2",
                                    url = "ST/getUsers"
                                },

                                new ApiViewNode() {
                                    name="tree4 ",
                                    url = "ST/get9User2"
                                },
                            }
                    }



                    }
                }, Models.AdminUserRole.SUPER_USER);

            







            AddMenu(new TreeNode()
            {
                name = "development",
                children = new List<ATreeNode>()
                    {
                        new PageViewNode() {
                            url = "/api-doc",
                            name="api-doc"
                        },
                        new PageViewNode() {
                            url = "/classDocs",
                            name="classDocs"
                        },
                        new EntityModel(){ Type= typeof(Models.Admin.AdminApiCall)},
                    }
            }, Models.AdminUserRole.DEVELOPER);

            AddMenu(new TreeNode()
            {
                name = "datas",
                children = new List<ATreeNode>()
                {
                    new EntityModel(){ Type= typeof(Models.Service)},
                    new EntityModel(){ Type= typeof(EnglishToefl.Models.Service)},
                    
                    new EntityModel(){ Type= typeof(Models.ServiceBuy)},
                    new EntityModel(){ Type= typeof(EnglishToefl.Models.ServiceBuy)},
                    
                    new EntityModel(){ Type= typeof(Models.ServiceUseLog)},
                    new EntityModel(){ Type= typeof(EnglishToefl.Models.ServiceUseLog)},
                    
                    
                    
                }
            }, Models.AdminUserRole.DEVELOPER);

            AddMenu(new TreeNode()
            {
                name = "development",
                children = new List<ATreeNode>()
                    {
                        new PageViewNode() {
                            url = "/api-doc",
                            name="api-doc"
                        },
                        new PageViewNode() {
                            url = "/classDocs",
                            name="classDocs"
                        },
                    }
            }, Models.AdminUserRole.DEVELOPER);



            return enitits;


        }
        public static async Task Main0(IServiceCollection Services)
        {





            //Program0.apis2[new Regex("EndUser/DoGameAction/[a-z_]+")] = typeof(FuncV<DoGameAction, bool>);
            //Program0.apis2[new Regex(@"trade/balance/"+ @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$")] = typeof(Func<Models.Bingx.Balance>);

            Program0.resources = typeof(Models.IIdMapper<int>).GetSubClasses(evenAbstracts: false);
            Program0.resources.AddRange(typeof(Models.IIdMapper<string>).GetSubClasses(evenAbstracts: false));
            Program0.resources.AddRange(typeof(Models.IIdMapper<System.Guid>).GetSubClasses(evenAbstracts: false));
            Program0.resources.AddRange(typeof(Models.IEntity0).GetSubClasses());

            Program0.resources.Add(typeof(EntityHistory<System.Guid>));


            Models.IAssetManager.instance = new DbManager();
            //Register Syncfusion license
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjM3OEAzMTM5MmUzMTJlMzBVeS82aFZBTTBzSG56NU1iekJscW9VN0s1UGJMcHBMRlFYMGduOUgxaUFvPQ==");

            Program0.GenerateWidgetsFunctions();
            Program0.GenerateForm();


            /*Program0.formRenderer[typeof(Segment)] = (List<Attribute> a) =>
            {
                return new SegmentEdit();
            };
            
            Program0.formRenderer[typeof(TradeBot)] = (List<Attribute> a) =>
            {
                return new UserFrom();
            };*/
            GenerateWidgetsFunctions();
            GenerateForm();


            //AddMenu(typeof(Models.Class));


            //Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            
            Services.AddBlazorise(options =>
            {
                options.Immediate = true;
            }).AddMaterialProviders()
                .AddMaterialIcons();
            //Services
            //.AddBlazoriseRichTextEdit(options => { });


            //builder.RootComponents.Add<App>("#app");


            //Services.AddSyncfusionBlazor();
            ClTool.WebClient.webClient = new ClTool2.WebClient("nothing" + "/");
            ClTool.WebClient.webClient.baseUrl = null;
        }
        public static async Task Main(string[] args)
        {


            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            //builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddCKEditor(builder.Configuration, options =>
            {
                options.ScriptPath = "https://cdn.ckeditor.com/ckeditor5/28.0.0/classic/ckeditor.js";
                options.StylePath = "https://ckeditor.com/docs/ckeditor5/latest/assets/styles.css";
                options.EditorClassName = "ClassicEditor";
                options.CKEditorConfig=new CKEditorConfig();
                
                //options.Theme = CKEditorTheme.Light;
            });
            await Main0(builder.Services);

            var host = builder.Build();

            var jsRuntime = host.Services.GetRequiredService<IJSRuntime>(); // get the service from the DI container
                                                                            // do something like get the culture - that's what the MS example for that does
            await Program0.CheckLogin(jsRuntime);

            await host.RunAsync();




        }




    }
}
