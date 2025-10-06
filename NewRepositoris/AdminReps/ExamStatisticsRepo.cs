using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientMsgs;
using Data.Data;
using EnglishToefl.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.TextTools;
using SGS.Core;
using Exam = Models.Exam;
using Response = Models.Response;

namespace Repositoris.AdminReps;

public class ExamStatisticsRepo(DBContext context)
{
    private readonly DBContext _context = context;

    
    
        private static List<int> findSquaresIndexes(string stringWithSquare, string originText)
        {
            // Improved: Work at the word level, since "■" never appears inside a word.

            // Helper to get word start indexes in a string
            List<int> GetWordStartIndexes(string text)
            {
                var indexes = new List<int>();
                bool inWord = false;
                for (int i = 0; i < text.Length; i++)
                {
                    if (char.IsLetterOrDigit(text[i]))
                    {
                        if (!inWord)
                        {
                            indexes.Add(i);
                            inWord = true;
                        }
                    }
                    else
                    {
                        inWord = false;
                    }
                }
                return indexes;
            }

            // New helper: splits into words and keeps "■" as a separate token
            List<string> ToWordsWithSquares(string text)
            {
                var words = new List<string>();
                var sb = new System.Text.StringBuilder();
                foreach (char c in text)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            words.Add(sb.ToString());
                            sb.Clear();
                        }
                        if (c == '■')
                        {
                            words.Add("■");
                        }
                        // ignore other non-word, non-square chars as word boundaries
                    }
                }
                if (sb.Length > 0)
                {
                    words.Add(sb.ToString());
                }
                return words;
            }

            var wordsWithSquare = ToWordsWithSquares(stringWithSquare);
            var wordsOrigin = ToWordsWithSquares(originText).Where(w => w != "■").ToList();
            var wordStartsOrigin = GetWordStartIndexes(originText);

            var result = new List<int>();
            int i = 0; // index for wordsWithSquare
            int j = 0; // index for wordsOrigin

            while (i < wordsWithSquare.Count)
            {
                if (wordsWithSquare[i] == "■")
                {
                    // Insert "■" at the start of the current word in originText (or at the end if j >= wordStartsOrigin.Count)
                    if (j < wordStartsOrigin.Count)
                        result.Add(wordStartsOrigin[j]);
                    else
                        result.Add(originText.Length);
                    i++;
                }
                else if (j < wordsOrigin.Count && wordsWithSquare[i] == wordsOrigin[j])
                {
                    // Words match, move both pointers
                    i++;
                    j++;
                }
                else
                {
                    // Smarter realignment: look ahead 3 words in both lists and find the first common word
                    int lookaheadLimit = 5;
                    var lookaheadWordsWithSquare = new HashSet<string>();
                    for (int k = 0; k < lookaheadLimit && (i + k) < wordsWithSquare.Count; k++)
                    {
                        lookaheadWordsWithSquare.Add(wordsWithSquare[i + k]);
                    }
                    int foundAtWordsOrigin = -1;
                    int foundAtWordsWithSquare = -1;
                    for (int k = 0; k < lookaheadLimit && (j + k) < wordsOrigin.Count; k++)
                    {
                        string candidate = wordsOrigin[j + k];
                        if (lookaheadWordsWithSquare.Contains(candidate))
                        {
                            foundAtWordsOrigin = j + k;
                            // Find where in wordsWithSquare this candidate is
                            for (int m = 0; m < lookaheadLimit && (i + m) < wordsWithSquare.Count; m++)
                            {
                                if (wordsWithSquare[i + m] == candidate)
                                {
                                    foundAtWordsWithSquare = i + m;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (foundAtWordsOrigin != -1 && foundAtWordsWithSquare != -1)
                    {
                        // Realign both pointers to the first common word
                        i = foundAtWordsWithSquare;
                        j = foundAtWordsOrigin;
                        i++;
                        j++;
                    }
                    else
                    {
                        // No common word found in lookahead, just advance i
                        i++;
                    }
                }
            }
            return result;
        }

      
        public async Task changeContent(Exam exam)
        {
            var qqs=await _context.Questions.Include(x=> x.section)
                .ThenInclude(x=> x.SectionParts.Where(x=> x.Content!=null))
                .Where(x => x.section.ExamId == exam.id && x.section.ExamPartType == ExamPartType.Reading)
                .GroupBy(x=> x.SectionId).ToListAsync();
            
            foreach (var qq0 in qqs)
            {

                var qq = qq0.ToList();
                
                Console.WriteLine(qq.Count);
                var sectionPart = qq[0].section.SectionParts.First();
                var pars = sectionPart.Content?.Split("\n").Where(s => s.Length >= 2).ToArray();
                var parsB = sectionPart.Content?.Split("\n").Where(s => s.Length >= 2).ToArray();
                //var pars22=pars.Select( x => get(x)).ToList();
                //sectionPart.paragraphs = pars22 ;
                Console.WriteLine(pars.Length);
                for (var i = 0; i < pars.Length; i++)
                {
                    pars[i].Split(". ");
                    pars[i] = $"[par-{i + 1}]{pars[i]}[par-{i + 1}/]";
                }

                sectionPart.Content2 = pars.Aggregate((l, r) => l + "\n\n" + r);
                _context.SectionParts.Entry(sectionPart).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                for (int i = 0; i < qq.Count; ++i)
                {
                    var question = qq[i];
                    
                    //if(!(question is BlankSpaceQuestion))
                    //    continue;
                    question.exteraContentExtension = null;
                    string QuestionContent = question.Content;
                    if(question.Content==null)
                        continue;
                    question.exteraContentExtension ??= [];
                    var pars2 = QuestionContent.Split("\n").Where(s => s.Length >= 2).ToArray();
                    if (Math.Abs(QuestionContent.Length - sectionPart.Content.Length) > sectionPart.Content.Length * 0.05)
                    {
                        //throw new Exception("5%");
                        Console.WriteLine("5%");
                        continue;
                    }


                    if(false){

                        var z = QuestionContent.IndexOf("🔷");
                        if (z > -1)
                        {
                            int endOfLine = QuestionContent.IndexOf('\n', z);
                            string s = QuestionContent.Substring(z + 2, endOfLine - z - 2);
                            int iddx = Array.FindIndex(pars, line => line == s);

                        }
                    }

                    {


                        var indexes = pars2
                            .Select((value, index) => new { value, index })
                            .Where(x => x.value.StartsWith("🔷"))
                            .Select(x => x.index)
                            .ToList();
                        if (!indexes.IsNullOrEmpty())
                        {
                            question.paragId = $"par-{indexes.First() + 1}";
                        }

                        foreach (var idx in indexes)
                        {
                            var pp = $"par-{idx + 1}";
                            question.exteraContentExtension.Add(
                                new Models.Question.TextExtension()
                                {
                                    id = $"q-{question.id}",
                                    type = Models.Question.TextExtension.Type.PARAG,
                                    start = new TextIndexPosition0()
                                    {
                                        fieldPath =
                                        [
                                            new TagFinder() { tag = pp },
                                        ],
                                        nextChar = 0
                                    },
                                    end = new()
                                    {
                                        fieldPath =
                                        [
                                            new TagFinder() { tag = pp + "/" },
                                        ],
                                        nextChar = 0
                                    }
                                }
                            );
                        }
                    }
                    foreach (var ss in (List<Tuple<string,Models.Question.TextExtension.Type>>) [
                                 new Tuple<string, Models.Question.TextExtension.Type>("```",Models.Question.TextExtension.Type.HIGHLIGHT),
                                 new Tuple<string,Models.Question.TextExtension.Type>("**",Models.Question.TextExtension.Type.BOLD)

                        ])
                        
                        for (var pi = 0; pi < pars2.Length; ++pi)
                        {
                            var par = pars2[pi];
                            var sectionPartPargraphTExt=parsB[pi];
                            var parTag = $"par-{pi+1}";
                            //string ss = "```";
                            
                            var startInQustionText = par.IndexOf(ss.Item1);
                            if (startInQustionText > -1)
                            {
                                int endInQuestionText = par.IndexOf(ss.Item1, startInQustionText + ss.Item1.Length, StringComparison.Ordinal);
                                {

                                    string selectedInQuestionText = par.Substring(startInQustionText + ss.Item1.Length,
                                        endInQuestionText - startInQustionText - ss.Item1.Length);
                                    string beforeText = par.Substring(0, startInQustionText);
                                    selectedInQuestionText = selectedInQuestionText.Trim();
                                    var startInSectionPartPargraphText = sectionPartPargraphTExt.IndexOf(
                                        selectedInQuestionText, int.Max(startInQustionText - 5, 0),
                                        StringComparison.Ordinal);
                                    Console.WriteLine(selectedInQuestionText);
                                    var words = beforeText.ToWords();
                                    var wordidx = words.Count; //sl.Split(" ").Count(s => s.Length > 0);
                                    var wordIdxEnd = wordidx + selectedInQuestionText.ToWords().Count - 1;

                                    question.exteraContentExtension ??= [];
                                    question.exteraContentExtension.Add(new Models.Question.TextExtension()
                                    {
                                        id = $"q-{question.id}",
                                        type = ss.Item2,
                                        start = new()
                                        {
                                            fieldPath =
                                            [
                                                new TagFinder() { tag = parTag },
                                                //new HtmlEscaperWordIndex() { idx = wordidx, wordPos = HtmlEscaperWordIndex.WordPos.start }
                                                new BracketEscaperIndex() { idx = startInSectionPartPargraphText }
                                            ],
                                            nextChar = 0
                                        },
                                        end = new()
                                        {
                                            fieldPath =
                                            [
                                                new TagFinder() { tag = parTag },
                                                //new HtmlEscaperWordIndex() { idx = wordIdxEnd, wordPos = HtmlEscaperWordIndex.WordPos.end },
                                                new BracketEscaperIndex()
                                                {
                                                    idx = startInSectionPartPargraphText + selectedInQuestionText.Length
                                                }

                                            ],
                                            nextChar = 0
                                        }
                                    });
                                }
                             
                                _context.Questions.Entry(question).State = EntityState.Modified;
                            }
                        }
                    
                    
                    for (var pi = 0; pi < pars2.Length; ++pi)
                    {
                        var par = pars2[pi];
                        var sectionPartPargraphTExt=parsB[pi];
                        var parTag = $"par-{pi+1}";
                        if (par.IndexOf("■") >= 0)
                        {
                            var indexes = findSquaresIndexes(par, sectionPartPargraphTExt);
                            foreach (var VARIABLE in indexes)
                                question.exteraContentExtension.Add(new Models.Question.TextExtension()
                                {
                                    id = $"q-{question.id}",
                                    type = Models.Question.TextExtension.Type.SQUARE,
                                    start = new()
                                    {
                                        fieldPath =
                                        [
                                            new TagFinder() { tag = parTag },
                                            //new HtmlEscaperWordIndex() { idx = wordidx, wordPos = HtmlEscaperWordIndex.WordPos.start }
                                            new BracketEscaperIndex() { idx = VARIABLE }
                                        ],
                                        nextChar = 0
                                    },
                                    end = new()
                                    {
                                        fieldPath =
                                        [
                                            new TagFinder() { tag = parTag },
                                            //new HtmlEscaperWordIndex() { idx = wordidx, wordPos = HtmlEscaperWordIndex.WordPos.start }
                                            new BracketEscaperIndex() { idx = VARIABLE }
                                        ],
                                        nextChar = 0
                                    }
                                });
                        }

                        _context.Questions.Entry(question).State = EntityState.Modified;
                    }

                    _context.SaveChanges();
                }
            }

        }

    
    public async Task examsChanger(int examId)
    {
        //var exams = context.Exams.Where(x => x.CompanyId == 10 ).OrderBy(x=> x.id).ToList();
                    
        //await changeContent(context, context.Exams.Find(1147));
        //var exams = context.Exams.Where(x => x.CompanyId == 2 || x.CompanyId == 10 ||x.CompanyId == 3 ).OrderBy(x=> x.id).ToList();

        var exam = await _context.Exams.FindAsync(examId);
            
                
        
            try
            {
                Console.WriteLine(exam.id);
        
                await changeContent(exam);
            }
            catch (Exception ex)
            {
                Console.WriteLine("--");
            }

        return ;
    }

    public async Task<bool> calcFirstExams( Guid uId, bool forceCalcAll=false)
    {
        /*await _context.ExamPartSessions.Where(x => x.CustomerId == uId && x.isFirst == null).ExecuteUpdateAsync(x =>
            x.SetProperty(
                examPartSession => examPartSession.isFirst,
                examP => _context.ExamPartSessions.Any(examPartSession => examPartSession.CustomerId == uId && examPartSession.examId == examP.examId && examPartSession.SectionType == examP.SectionType && examPartSession.startTime < examP.startTime)
            )
        );*/
        
        
        var firstTimes = _context.ExamPartSessions
            .Where(x => x.CustomerId == uId)
            .GroupBy(x => new { x.examId, x.SectionType })
            .Select(g => new
            {
                g.Key.examId,
                g.Key.SectionType,
                minStartTime = g.Min(x => x.startTime)
            });
        var q2 = _context.ExamPartSessions
            .Where(x => x.CustomerId == uId);
        if (!forceCalcAll)
            q2=q2.Where(x => x.isFirst == null);
        await q2.Join(firstTimes, 
                exam => new { exam.examId, exam.SectionType },
                ft => new { ft.examId, ft.SectionType },
                (exam, ft) => new { exam, ft })
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    x => x.exam.isFirst,
                    j => (j.exam.startTime - j.ft.minStartTime).Seconds<10 || (j.exam.startTime - j.ft.minStartTime).Seconds>-10
                )
            );

        

        return true;
    }



    public async Task<Dictionary<int, long>> getTime(ExamPartSessionLogs tw)
    {
        
        
        var qs = await _context.Responses.Where(x => x.examPartSessionId == tw.id).Select(x => x.QuestionId)
            .ToListAsync();
        var qs2 =new  Dictionary<int, long>();
        foreach (var v in qs)
        {
            qs2[v] = 0;
        }
        tw.getQuestionsTime(qs2);
        return qs2;
    }
    public async Task<bool> calcQuestionsStatistics( int examId, bool forceCalcAll=false)
        {


            //var q=_context.Questions.Where(x => x.id == qId).Include(x => x.section).First();
            var examStat=await _context.examStatistics.Where(x=> x.id==examId).FirstOrDefaultAsync();
            if (examStat == null)
            {
                examStat = new ExamStatistics()
                {
                    id = examId,
                    lastCalcTime = DateTime.MinValue
                };
                
                _context.examStatistics.Add(examStat);
                await _context.SaveChangesAsync();
            }

            if (forceCalcAll)
            {
                examStat.lastCalcTime = DateTime.MinValue;
            }

            examStat.lastCalcTime = DateTime.MinValue;
            /*var result =(from log in _context.examPartSessionLogs
                join exam in _context.ExamPartSessions 
                    on log.id equals exam.id
                where exam.examId == examId && exam.isFirst.Value && exam.startTime>examStat.lastCalcTime
                orderby exam.startTime descending
                select log)
                .Take(1000);*/

            var result = (from log in _context.examPartSessionLogs
                    join exam in _context.ExamPartSessions
                        on log.id equals exam.id
                    join response in _context.Set<Response>()
                        on exam.id equals response.examPartSessionId
                    where exam.examId == examId
                          && exam.isFirst.Value
                          && exam.startTime > examStat.lastCalcTime
                          && !response.IsRemoved
                    orderby exam.startTime descending
                    select new
                    {
                        SessionLog = log,
                        Session = exam,
                        Responses = _context.Set<Response>()
                            .Where(r => r.examPartSessionId == exam.id)
                            .ToList()
                    })
                .Take(10);

            {
                var aa = await _context.ExamPartSessions.Where(x =>
                        x.isFirst.Value
                        && x.examId == examId
                        && x.SectionType == ExamPartType.Reading
                    ).GroupBy(x => x.score)
                    .Select(x =>
                        new
                        {
                            score = x.Key,
                            number = x.Count()
                        }).ToListAsync();
                if(examStat.readingScores==null)
                    examStat.readingScores=new Dictionary<int, int>();
                foreach (var a in aa)
                {
                    if (a.score.HasValue)
                        examStat.readingScores[a.score.Value] = a.number;
                }
                
            }
            {
                var aa = await _context.ExamPartSessions.Where(x =>
                        x.isFirst.Value
                        && x.examId == examId
                        && x.SectionType == ExamPartType.Listening
                    ).GroupBy(x => x.score)
                    .Select(x =>
                        new
                        {
                            score = x.Key,
                            number = x.Count()
                        }).ToListAsync();
                if(examStat.ListeningScores==null)
                    examStat.ListeningScores=new Dictionary<int, int>();
                foreach (var a in aa)
                {
                    if (a.score.HasValue)
                        examStat.ListeningScores[a.score.Value] = a.number;
                }
            }


            //examStat.lastCalcTime = DateTime.UtcNow;
            var logsList = await result.ToListAsync();
            examStat.lastCalcTime = DateTime.UtcNow;
            _context.examStatistics.Entry(examStat).State = EntityState.Modified;
            
            
            
            Dictionary<int, Tuple<int,long>> qsTime = new ();
            Dictionary<int, Dictionary<int,long>> qsoptions = new ();
            foreach (var sp in logsList)
            {
                
                var qs=await getTime(sp.SessionLog);
                foreach (var i in qs)
                {
                    if(i.Value<5000)
                        continue;
                    if (!qsTime.ContainsKey(i.Key))
                    {
                        qsTime[i.Key] = new Tuple<int, long>(0, 0);
                        
                    }

                    qsTime[i.Key]=new Tuple<int, long>(qsTime[i.Key].Item1+1,qsTime[i.Key].Item2+i.Value);
                }
                foreach (var i in sp.Responses)
                {
                    if (!qsoptions.ContainsKey(i.QuestionId))
                    {
                        qsoptions[i.QuestionId] = new Dictionary<int, long>();
                    }

                    if (i is QuestionTrueFalseOptionResponse tr)
                    {
                        foreach (var to in tr.answer)
                        {
                            if(!qsoptions[i.QuestionId].ContainsKey(to.Value))
                                qsoptions[i.QuestionId][to.Value]=0;
                            qsoptions[i.QuestionId][to.Value]+=1;
                        }
                    }

                    //qsoptions[i.QuestionId]=new Tuple<int, long>();
                }
                
            }

            foreach (var i in qsTime)
            {
                
                
                
                var qq=await _context.questionStatistics.FindAsync(i.Key);
                
                if (qq == null)
                {
                    qq = new QuestionStatistics()
                    {
                        id = i.Key,

                    };
                    _context.questionStatistics.Add(qq);
                    await _context.SaveChangesAsync();
                }
                if (forceCalcAll)
                {
                    qq.correctAnswerSumTime = 0;
                    qq.incorrectAnswerSumTimeNumber = 0;
                    qq.correctAnswerSumTimeNumber = 0;
                    qq.optionsNumber = new Dictionary<int, long>();
                }
                if(qsoptions.ContainsKey(i.Key))
                    foreach (var j in qsoptions[i.Key])
                    {
                        qq.optionsNumber[j.Key]=j.Value;
                    }
                
                qq.correctAnswerSumTime += i.Value.Item2;
                qq.correctAnswerSumTimeNumber += i.Value.Item1;
                _context.questionStatistics.Entry(qq).State = EntityState.Modified;
            }

            
            await _context.SaveChangesAsync();


            return true;
        }


    public void f()
    {
       /* var questionStatistics=await context.questionStatistics.Where(x=> context.Questions.Where(x=> x.SectionId==sectionId).Select(x=> x.id).Contains(x.id)).ToListAsync();
        questionStatistics.sc
        foreach (var questionStatistic in questionStatistics)
        {
            questionStatistic.correctAnswerSumTimeNumber * question[questionStatistic.id].mark;
        }*/
    }
    public async Task<bool> calcSectionStatistics(int sectionId, bool forceCalcAll = false)
    {
        // Find or create SectionStatistics for this section
        var sectionStatics = await context.sectionStatistics.FindAsync(sectionId);
        if (sectionStatics == null)
        {
            sectionStatics = new SectionStatistics() { id = sectionId ,lastCalcTime = DateTime.MinValue};
            context.sectionStatistics.Add(sectionStatics);
            context.SaveChanges();
        }

        // Get all UserSectionData for this section
        var userSectionDatas = await context.userSectionDatas
            .Where(x => x.sectionId == sectionId)
            .ToListAsync();

        // Calculate histogram of correctResponsesNumber (rounded to int)
        var histogram = userSectionDatas
            .GroupBy(x => (int)Math.Round(x.correctResponsesNumber))
            .ToDictionary(g => g.Key, g => g.Count());

        // Update the scores property
        sectionStatics.scores = histogram;

        // Update lastCalcTime
        sectionStatics.lastCalcTime = DateTime.UtcNow;

        context.sectionStatistics.Update(sectionStatics);
        await context.SaveChangesAsync();

        return true;
    }
    
}