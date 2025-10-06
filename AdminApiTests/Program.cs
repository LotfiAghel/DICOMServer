using System;
using Models;



using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AdminMsg;
using System.Linq;
using Models.AiResponse;
using Models.Exams;
using OpenAI_API;
using OpenAI_API.Models;
using OpenAI_API.Chat;

using Models.Queris;
using TestHelperAI;


namespace ApiCall
{

    class Program
    {

        public static async Task<string> AiTest()
        {
            var api = Settings.GetDefaultOpenAIAPI(); 
            var chat = api.Chat.CreateConversation();
            chat.Model = "gpt-4o";
            chat.RequestParameters.Temperature = 0;

            /// give instruction as System
            chat.AppendSystemMessage("You are a teacher who helps children understand if things are animals or not.  If the user tells you an animal, you say \"yes\".  If the user tells you something that is not an animal, you say \"no\".  You only ever respond with \"yes\" or \"no\".  You do not say anything else.");

            // give a few examples as user and assistant
            chat.AppendUserInput("Is this an animal? Cat");
            chat.AppendExampleChatbotOutput("Yes");
            chat.AppendUserInput("Is this an animal? House");
            chat.AppendExampleChatbotOutput("No");

            // now let's ask it a question
            chat.AppendUserInput("Is this an animal? Dog");
            // and get the response
            string response = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response); // "Yes"

            // and continue the conversation by asking another
            chat.AppendUserInput("Is this an animal? Chair");
            // and get another response
            response = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                // Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response;
        }
        static private OpenAIAPI api = Settings.GetDefaultOpenAIAPI();
        //static Conversation chat;
        public static async Task<Conversation> createAsistant()
        {
            if (api == null)
                api = Settings.GetDefaultOpenAIAPI();
            
            
            
            var chat = api.Chat.CreateConversation();
            chat.Model = "gpt-4o";
            chat.RequestParameters.Temperature = 0;

            /// give instruction as System
            chat.AppendSystemMessage("You are an English TOEFL test teacher who helps students understand why the specific options of questions are true." +
                " \r\nuser will  send request as json data with " +
                "{\"question\":%question text%,\"paragraph\":%paragraph texts%,options:[%option A%,%option B%,%option C%,%option D%],\"trueOption\":%A or B or C or D%}" +
                " format\r\n questions with paragraph options and correct option.\r\n you should say why the option is correct and you should select part of the paragraph where you conclude this option is true");
            /*if (false)
            {
                // give a few examples as user and assistant
                chat.AppendUserInput(JToken.FromObject(new
                {
                    question = qustion.QuestionText,
                    paragraph = qustion.contents,
                    options = qustion.questionOptions.Select(x => x.Content),
                    trueOption = trueOption.ToString()

                }).ToString());
                chat.AppendExampleChatbotOutput("Yes");
                chat.AppendUserInput("Is this an animal? House");
                chat.AppendExampleChatbotOutput("No");

                // now let's ask it a question
                chat.AppendUserInput("Is this an animal? Dog");
                // and get the response
                string response = await chat.GetResponseFromChatbotAsync();
                Console.WriteLine(response); // "Yes"
            }*/
            return chat;
        }
        public static  Conversation createAsistant2()
        {
            if (api == null)
                api = Settings.GetDefaultOpenAIAPI(); 
            var chat = api.Chat.CreateConversation();
            chat.Model = "gpt-4o";
            chat.RequestParameters.Temperature = 0;
            var optionsS = "[% option A %,% option B %,% option C %,,% option D %,% option E %,% option F %,% option G %,% option H %,% option I %]";
            /// give instruction as System
            chat.AppendSystemMessage("You are an English TOEFL test teacher who helps students understand why the specific options of questions are true." +
                " \r\nuser will  send request as json data with " +
                $"{{\"question\":%question text%,\"context\":%context texts%,options:{optionsS},\"correctOptions\":{optionsS},\"correctOptionsNumber\":%correctOptionsNumber% }}" +
                " format\r\n questions with context options and correct options and correct options number.\r\n you should say why the options is correct or not .and you should select part of the context where you conclude this options is true");
            return chat;
        }
        public static  Conversation createAsistantWordQ2()
        {
            if (api == null)
                api = Settings.GetDefaultOpenAIAPI(); 
            var chat = api.Chat.CreateConversation();
            chat.Model = "gpt-4o";
            chat.RequestParameters.Temperature = 0;
            var words = "[% word 1 %, % word 2 if exist %, ...]";

            chat.AppendSystemMessage(
                "You are an IELTS English test teacher. Your role is to explain to students why the given correct word(s) are the right answer to the question.\r\n" +
                "The user will send data as JSON in the format:\r\n" +
                $"{{\"question\":%question text%,\"context\":%context text%,\"correctWords\":{words}}}\r\n" +
                "Notes:\r\n" +
                "- The 'correctWords' array may contain one or more items.\r\n" +
                "- A word may include a simple optional part using parentheses, e.g., 1992(th), meaning both '1992' and '1992th' are acceptable.\r\n" +
                "Your task:\r\n" +
                "1. Identify the relevant part(s) of the context that justify the answer.\r\n" +
                "2. If a word contains an optional part, explain both variants in a single explanation.\r\n" +
                "3. Keep your explanation clear, concise, and focused on IELTS reasoning.\r\n" +
                "4. Provide only the necessary context snippet(s) to support your reasoning."
            );
            return chat;
        }

        public static Conversation createAsistantL()
        {
            var chat = api.Chat.CreateConversation();
            chat.Model = "gpt-4o";
            chat.RequestParameters.Temperature = 0;
            var optionsS = "[% option A %,% option B %,% option C %,,% option D %]";
            /// give instruction as System
            chat.AppendSystemMessage("You are an English TOEFL test teacher who helps students understand why the specific options of questions are true." +
                " \r\nuser will  send request as json data with " +
                $"{{\"question\":%question text%,\"conversion\":%conversion transcript%,options:{optionsS},\"correctOption\": %option% }}" +
                " format\r\n questions with conversion transcript , options and correct option ."+
                "conversion transcript is a text of dialog between multipe person"+
                "\r\n you should say why the option is correct  .and you should select part of the context where you conclude this option is true");
            return chat;
        }
        public static async Task<string> AiTest2(string parg, Models.QuestionOptinalSingle qustion, OptionTitle trueOption)
        {
            var chat=await createAsistant();
            chat.AppendUserInput(JToken.FromObject(new
            {
                question = qustion.QuestionText,
                paragraph = parg,
                options = qustion.questionOptions.Select(x => x.Content),
                trueOption = trueOption.ToString()

            }).ToString());
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                // Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }
        
        
        public static async Task<string> AiTest2IELTS(string parg, Models.QuestionOptinalSingle qustion, OptionTitle trueOption)
        {
            var chat=await createAsistant();
            chat.AppendUserInput(JToken.FromObject(new
            {
                question = qustion.QuestionText,
                paragraph = qustion.ComputedMobileContent,
                options = qustion.questionOptions.Select(x => x.Content),
                trueOption = trueOption.ToString()

            }).ToString());
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                // Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }

        
        public static async Task<string> AiTest3(string parg, Models.QuestionTrueFalseOption qustion, List<OptionTitle> trueOptions)
        {
            var chat = createAsistant2();
            var msg2 = JToken.FromObject(new
            {
                question = qustion.QuestionText,
                paragraph = parg,
                options = qustion.questionOptions.Select(x => $"{x.title.ToString()}: {x.Content}"),
                correctOptions = trueOptions.Select(x => x.ToString()),
                correctOptionsNumber = trueOptions.Count()

            }).ToString();
            Console.WriteLine(msg2);
            chat.AppendUserInput(msg2);
            Console.WriteLine("go to GPT-4 -----------------------------------------------------------");
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                // Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }


        public static async Task<string> AiTestL2(string parg, Models.QuestionOptinalSingle qustion, OptionTitle trueOption)
        {
            var chat= createAsistantL();
            string subtitiles = null;
            try
            {
                subtitiles = qustion.section.SectionParts.First().subtitile.Select(x => x.text + "\n").Aggregate((l, r) => $"{l},{r}");
            }
            catch
            {

            }
            if (subtitiles == null)
            {
                subtitiles = qustion.section.SectionParts.First().Content;
            }
            chat.AppendUserInput(JToken.FromObject(new
            {
                question = qustion.QuestionText,
                conversion = subtitiles,
                options = qustion.questionOptions.Select(x => x.Content),
                trueOption = trueOption.ToString()

            }).ToString());
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }
        
            
        public static async Task<string> AiTestLWordQ(string parg, WordQuestion qustion, List<string> trueOptions,
            string qpTxt)
        {
            var chat = createAsistantWordQ2();
            var js = new
            {
                question = qpTxt,
                paragraph = parg,
                options = qustion.answerSheet,
                correctWords = trueOptions,
                

            };
            var msg2 = JToken.FromObject(js).ToString();
            Console.WriteLine(msg2);
            chat.AppendUserInput(msg2);
            Console.WriteLine("go to GPT-4 -----------------------------------------------------------");
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }
        public static async Task<string> AiTestL3(string parg, Models.QuestionTrueFalseOption qustion, List<OptionTitle> trueOptions)
        {
            var chat = createAsistant2();
            var js = new
            {
                question = qustion.QuestionText,
                paragraph = parg,
                options = qustion.questionOptions.Select(x => $"{x.title.ToString()}: {x.Content}"),
                correctOptions = trueOptions.Select(x => x.ToString()),
                correctOptionsNumber = trueOptions.Count()

            };
            var msg2 = JToken.FromObject(js).ToString();
            Console.WriteLine(msg2);
            chat.AppendUserInput(msg2);
            Console.WriteLine("go to GPT-4 -----------------------------------------------------------");
            var response2 = await chat.GetResponseFromChatbotAsync();
            Console.WriteLine(response2); // "No"

            // the entire chat history is available in chat.Messages
            foreach (ChatMessage msg in chat.Messages)
            {
                 Console.WriteLine($"{msg.Role}: {msg.Content}");
            }
            return response2;
        }

        public static async Task proccess(ClTool.WebClient cl, Models.Section sec)
        {
            var ex = new GenericAdminController<Models.Section, int>(cl);
            var questionControler = new GenericAdminController<Models.Question, int>(cl);
            var questions = await ex.getAllDetailClass2(sec, x => x.questions);

            //var singleOptionResponse = questions.OfType<QuestionOptinalSingle>();
            var singleOptionResponse = questions.OfType<QuestionTrueFalseOption>();
            var contents = new GenericAdminController<Models.AiResponse.Content, Guid>(cl);
           
            foreach (var reading in singleOptionResponse)
                try
                {
                    //if (reading is QuestionOptinalSingle)
                    //    continue;
                    
                    await proccessQ(questionControler,contents, reading,null);
                }
                catch
                {

                }
        }
        
        private static async Task proccessQ(GenericAdminController<Models.Question, int> questionControler,
            GenericAdminController<Models.AiResponse.Content, Guid> con, Models.QuestionTrueFalseOption reading
            ,string content)
        {
            /*var conts = await questionControler.getAllDetailClass2(reading, x => x.contents);
            if (conts.Count() > 0)
                return;*/

            while (true)
                try
                {
                    string res = null;
                    {
                        if (res ==null && reading is Models.QuestionOptinalSingle reading2)
                            res=await AiTest2(content, reading2, reading.questionOptions.Find(x => x.IsCorrect).title);
                    }
                    {


                        if (res == null && reading is Models.QuestionTrueFalseOption reading2)
                            res=await AiTest3(content, reading2, reading.questionOptions.Where(x => x.IsCorrect).Select(x=> x.title).ToList());
                    }
                    
                    // break;

                    await con.post(new Models.AiResponse.Content()
                    {
                        id = Guid.Empty,
                        QuestionId = reading.id,
                        Description = res,
                    });
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

        }
        private static async Task proccessWQ(GenericAdminController<Question, int> questionControler,
            GenericAdminController<Content, Guid> con, Question reading
            , string parag
            , string qpTxt)
        {
            /*var conts = await questionControler.getAllDetailClass2(reading, x => x.contents);
            if (conts.Count() > 0)
                return;*/

            while (true)
                try
                {
                    string res = null;
                    {
                        if (res ==null && reading is Models.WordQuestion reading2)
                            res=await AiTestLWordQ(parag, reading2, reading2.answerSheet,qpTxt);
                    }
                   
                    
                    // break;

                    await con.post(new Models.AiResponse.Content()
                    {
                        id = Guid.Empty,
                        QuestionId = reading.id,
                        Description = res,
                    });
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

        }
        private static async Task proccesslQ(GenericAdminController<Models.Question, int> questionControler, GenericAdminController<Models.AiResponse.Content, Guid> con, Models.QuestionTrueFalseOption reading)
        {

            var paragraph = reading.section.SectionParts.First().subtitile.Select(x => x.text + "\n")
                .Aggregate((l, r) => $"{l},{r}");
            //while (true)
                try
                {
                    string res = null;
                    {
                        if (res == null && reading is Models.QuestionOptinalSingle reading2)
                            res = await AiTestL2(reading.QuestionText, reading2, reading.questionOptions.Find(x => x.IsCorrect).title);
                    }
                    {


                        if (res == null && reading is Models.QuestionTrueFalseOption reading2)
                            res = await AiTestL3(paragraph, reading2, reading.questionOptions.Where(x => x.IsCorrect).Select(x => x.title).ToList());
                    }
                   
                    // break;

                    await con.post(new Models.AiResponse.Content()
                    {
                        id = Guid.Empty,
                        QuestionId = reading.id,
                        Description = res,
                    });
                   // break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

        }
        private static async Task proccesslQ(GenericAdminController<Models.Question, int> questionControler, GenericAdminController<Models.AiResponse.Content, Guid> con, Models.Question reading)
        {

            //while (true)
            try
            {
                string res = null;
                
                {


                    if (reading is Models.WordQuestion reading2)
                        res = await AiTestLWordQ(reading.Content, reading2, reading2.answerSheet,null);
                }
                // break;

                await con.post(new Models.AiResponse.Content()
                {
                    id = Guid.Empty,
                    QuestionId = reading.id,
                    Description = res,
                });
                // break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static async Task AiTest(ClTool.WebClient cl, Models.Exam exam)
        {
            var ex = new GenericAdminController<Models.Exam, int>(cl);
            var sect = new GenericAdminController<Models.SectionType, int>(cl);
            //await tz.migrate();
            var sectionTypes = await sect.getAll();

            var sections = await ex.getAllDetailClass2(exam, x => x.Sections);
            var readingSEctions = sections.Where(x => sectionTypes.Find(y => y.id == x.SectionTypeId).ExamPartType == EnglishToefl.Models.ExamPartType.Reading);
            foreach (var reading in readingSEctions)
                try
                {
                    
                    await proccess(cl, reading);
                }
                catch
                {

                }
        }
        public static string lback = "https://admin-server.oncodraw.com/";
        //public static string lback = "https://localhost:3011/";
        //public static string lback = "https://localhost:3011/";
        public static async Task helthCheck(ClTool.WebClient lbackWebClient)
        {

            
            var uc = new PublicService(lbackWebClient);
            while (true)
            {
                try
                {

                    await uc.healthCheck();
                    break;

                }
                catch
                {

                }
            }
        }
       
        public static async Task Main(string[] args)
        {

            //await GrammerAi.createAsistant();
            //string res;
            //res = await GrammerAi.AiTest2("i am go to the school and saw the my frieend and I said to him why you are there");
            // res = await GrammerAi.AiTest2("i will go to the school and saw the my frieend");
            //res = await GrammerAi.AiTest2(" You are an english llanguage editor who helps the user to write better and fix spelling and grammar mistakes.\r\n");


            //lback = "https://alfa-admin.oncodraw.com/";
            var cc = ClTool.WebClient.webClient = new ClTool.WebClient(lback);
            await helthCheck(cc);
            var tz = new AdminUserController(cc);
            await tz.login(new LoginRequest
            {
                userName = "machinAdmin",
                pass="dicom"
            });

            //await MemoryCheck(cc);
            var ex = new GenericAdminController<Models.Exam, int>(cc);
            var sectionControler = new GenericAdminController<Models.Section, int>(cc);
            var questionControler = new GenericAdminController<Models.Question, int>(cc);
            var sectionPartControler = new GenericAdminController<Models.Exams.SectionPart, int>(cc);
            var contentControler=new GenericAdminController<Models.AiResponse.Content, Guid>(cc);
            
            //await tz.migrate();
            var exams = await ex.getAll();
            //await AiTest(t, exams.Find(x => x.id == 2));
            if (true)
            {
                var qa = await questionControler.GetEntitys2(new IQueryContainer<Question>()
                {
                    query = new QuestionNotHaveContent()
                    {
                        inComponay = [ 2,10 ],//,15,31,36],//[ 2,10 ],
                        parts = [EnglishToefl.Models.ExamPartType.Listening],
                    }
                });
              
                
                
                var qa2 = qa.OrderBy(x => x.id).ToList();
                //await createAsistant();
                int i = 0;
                foreach (var q in qa2) try
                    {
                        ++i;
                        q.section = await sectionControler.get(q.SectionId);
                        if (q.section.ExamPartType != EnglishToefl.Models.ExamPartType.Listening)
                            continue;
                        var sectionParts=q.section.SectionParts = await sectionControler.getAllDetailClass2(q.section,x => x.SectionParts);
                        
                        var paragraph = sectionParts.First().subtitile.Select(x => x.text + "\n")
                            .Aggregate((l, r) => $"{l},{r}");
                        Console.WriteLine($"{i}/{qa.Count()}");
                        if (q is WordQuestion)
                        {
                            
                            var questionPacks=await sectionControler.getAllDetailClass2<QuestionPack>(q.section,x => x.questionPacks);
                            q.section.questionPacks = questionPacks;
                            foreach(var qp in questionPacks)
                            {
                                if (qp.questions.ToList().Any(x => x.Value == q.id))
                                {
                                    var tmp=qp.htmlContent.Replace($"[i[={q.id}=]]","% word place %");
                                    await proccessWQ(questionControler,contentControler, q,paragraph,tmp);
                                    break;
                                }
                            }
                            
                        }
                        else
                            await proccesslQ(questionControler,contentControler, q );
                    }
                    catch(Exception ex2)
                    {
                        Console.WriteLine(ex2.Message);
                    }
            }
            
            if (true)
            {
                var qa = await questionControler.GetEntitys2(new IQueryContainer<Question>()
                {
                    query = new QuestionNotHaveContent()
                    {
                        inComponay = [2,10],
                        parts = [EnglishToefl.Models.ExamPartType.Reading]
                    }
                });
                
                qa=qa.OrderBy(x => x.id).ToList();
                //await createAsistant();
                int i= 0; 
                foreach (var q in qa) try
                    {
                        ++i;
                        Console.WriteLine($"{i}/{qa.Count()}");
                        
                        var section=await sectionControler.get(q.SectionId);
                        var sectionpart=await sectionControler.getAllDetailClass2<SectionPart>(section,x => x.SectionParts);
                        var questionPacks=await sectionControler.getAllDetailClass2<QuestionPack>(section,x => x.questionPacks);
                        q.section = section;
                        q.section.SectionParts = sectionpart;
                        await proccessQ(questionControler,contentControler, q as QuestionTrueFalseOption, sectionpart[0].Content);
                        //await proccessQ(questionControler,contentControler, q as QuestionTrueFalseOption,q.ComputedMobileContent);
                    }
                    catch
                    {

                    }
            }
            
            if (true)
            {

                var qa = await questionControler.GetEntitys2(new IQueryContainer<Question>()
                {
                    query = new QuestionNotHaveContent()
                    {
                        inComponay = [15,31,36],
                        //inExams = [2118],
                        parts = [EnglishToefl.Models.ExamPartType.Reading],
                    }
                });
                qa=qa.OrderBy(x => x.id).ToList();
                //await createAsistant();
                int i= 0; 
                foreach (var q in qa) try
                    {
                        ++i;
                        Console.WriteLine($"{i}/{qa.Count()}");
                        
                        var section=await sectionControler.get(q.SectionId);
                        var sectionpart=await sectionControler.getAllDetailClass2<SectionPart>(section,x => x.SectionParts);
                        var questionPacks=await sectionControler.getAllDetailClass2<QuestionPack>(section,x => x.questionPacks);
                        q.section = section;
                        q.section.SectionParts = sectionpart;
                        q.section.questionPacks = questionPacks;
                        foreach(var qp in questionPacks)
                        {
                            if (qp.questions.ToList().Any(x => x.Value == q.id))
                            {
                                var tmp=qp.htmlContent.Replace($"[i[={q.id}=]]","% word place %");
                                await proccessWQ(questionControler,contentControler, q,sectionpart[0].Content,tmp);
                                break;
                            }
                        }
                        
                        //await proccessQ(questionControler,contentControler, q as QuestionTrueFalseOption,q.ComputedMobileContent);
                    }
                    catch
                    {

                    }
            }

            return ;
            await tz.migrateExam(3);
            await tz.migrateExam(77);
            //await tz.migrateExam(1220);
            //await tz.migrateExam(1139);
            /*for (int i = 1191; i < 1191 + 10; ++i)
            {
                await tz.migrateExam(i);
            }*/

            foreach (var e in exams.OrderBy(x => x.id))
            {
                await tz.migrateExam(e.id);
            }




        }
    }
    
    
    
}
