using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.Types;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Utils;

// ReSharper disable VariableHidesOuterVariable

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly HSE_QuestContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly Random Random = new Random();

        public QuestionsController(HSE_QuestContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Questions
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("ListQuestions", "Display");
        }

        // GET: Questions/Details/id
        [AllowAnonymous]
        public async Task<ActionResult> Details(int id)
        {
            var question = await DataStorage.GetByPredicateAsync(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            await DataStorage.GetByPredicateAsync(_context.TypesInfo,
                typesInfo => typesInfo.TypeId == question.TypeId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);

            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(_context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var metaTags = new HashSet<int>();

            foreach (var tagQuestion in tagsQuestions)
            {
                metaTags.UnionWith(
                    await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(_context.TagsInfo,
                        tagsInfo => tagsInfo.TagId == tagQuestion.TagId && tagsInfo.IsMetaTag == true,
                        tagsInfo => tagsInfo.TagId));
            }

            ViewData["MetaTags"] = metaTags;

            return View(question);
        }

        // GET: Questions/Create
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewData["Tags"] = await DataStorage.GetHashSetAsync(_context.TagsInfo);

            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var question = await QuestionExtensions.CreateQuestion(_context,
                    collection[QuestionText], collection[TypeInfo],
                    collection[QuestionName], collection["IsTemplate"]);

                await StorageUtils.SaveToDatabase(_context, question);

                await AddResponseOptions(collection[TypeInfo][0], question.QuestId,
                    collection[AnswerText].ToArray(), collection["Correct"].ToArray());

                await AddTagsToDatabase(collection["Tags"], question.QuestId, question);

                return RedirectToAction("Details", new {id = question.QuestId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        // GET: Questions/Edit/id
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var question = await DataStorage.GetByPredicateAsync(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            ViewData["CurrentTags"] = await GetTags(question);
            ViewData["AllTags"] = await DataStorage.GetHashSetAsync(_context.TagsInfo);

            return View(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                var typeName = collection["Type.Name"][0];

                var question = await DataStorage.GetByPredicateAsync(
                    _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

                var sourceQuestionId = await QuestionExtensions.GetSourceVersionId(_context, question);

                var newQuestion = await QuestionExtensions.CreateQuestion(_context,
                    collection["QuestionName"], collection["QuestionText"],
                    typeName, await GetVersion(sourceQuestionId), sourceQuestionId);

                await StorageUtils.SaveToDatabase(_context, newQuestion);

                await AddTagsToDatabase(collection["Tags"], newQuestion.QuestId);

                await AddResponseOptions(typeName, newQuestion.QuestId, collection[AnswerText].ToArray(),
                    collection["Correct"].ToArray());

                return RedirectToAction("Details", new {id = newQuestion.QuestId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        public async Task<IActionResult> ExportToXml(int id)
        {
            var question = await DataStorage.GetByPredicateAsync(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (question.QuestionXml != null)
            {
                return File(Encoding.UTF8.GetBytes(question.QuestionXml),
                    "application/xml", $"question{id}.xml");
            }

            var responseOptions = await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == id);

            var document = XmlGenerator.ExportToXml(question, responseOptions, _environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", $"question{id}.xml");
        }

        public IActionResult Display(int id) =>
            RedirectToAction("ListByTag", "Display", new {id});

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            ViewData["Tags"] = await DataStorage.GetHashSetAsync(_context.TagsInfo);

            return View();
        }

        public async Task<IActionResult> Export(IFormCollection collection)
        {
            ViewData["Tags"] = await DataStorage.GetHashSetAsync(_context.TagsInfo);

            if (!collection["Tags"].Any())
            {
                return View();
            }

            var tagIdentifiers = collection["Tags"].Select(int.Parse).ToHashSet();

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                tagsQuestions => tagIdentifiers.Contains(tagsQuestions.TagId));

            var questionIdentifiers = GetQuestionIdentifiers(tagsQuestions, collection["QuestionsAmount"]);

            var questions = await DataStorage.GetListByPredicateAsync(_context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId));

            var responseOptions = new List<List<QuestionAnswerVariants>>();

            foreach (var questionId in questionIdentifiers)
            {
                responseOptions.Add(await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                    questionAnswerVariants => questionAnswerVariants.QuestId == questionId));
            }

            var document = XmlGenerator.ExportQuestionsToXml(questions, responseOptions, _environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", "questions.xml");
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "XML file is required.");
                return View("Import");
            }

            try
            {
                var xmlData = await file.ReadAsStringAsync();
                var document = new XmlDocument();
                document.LoadXml(xmlData);

                var type = TypesExtensions.GetTypeIdFromFullName(XmlGenerator.FindQuestionType(document));

                var question = await QuestionExtensions.CreateQuestion(_context,
                    XmlGenerator.GetElementTextFromXml(document, "questiontext"), type, 
                    XmlGenerator.GetElementTextFromXml(document, "name"), xmlData);

                await StorageUtils.SaveToDatabase(_context, question);

                if (question.TypeId != 4)
                {
                    var (responseOptions, correct) = XmlGenerator.GetResponseInfo(document);
                    await AddResponseOptions(TypesExtensions.GetTypeId(type), question.QuestId, responseOptions, correct);
                }

                return RedirectToAction("Details", new {id = question.QuestId});
            }
            catch (XmlException ex)
            {
                ModelState.AddModelError("File", ex.Message);
                return View("Import");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int id)
        {
            var question = await DataStorage.GetByPredicateAsync(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await DataStorage.GetByPredicateAsync(_context.QuestionAnswerVariants, 
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            await DataStorage.GetByPredicateAsync(_context.TypesInfo, 
                typesInfo => typesInfo.TypeId == question.TypeId);
            
            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions, 
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);
            
            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(_context.TagsInfo, 
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var model = new TemplateQuestionViewModel {Question = question};

            return View(model);
        }

        public async Task<IActionResult> Generate(IFormCollection collection, int id)
        {
            var template = await DataStorage.GetByPredicateAsync(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (template == null)
            {
                return ErrorPage(404);
            }

            var answers = await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == template.QuestId);

            await DataStorage.GetByPredicateAsync(_context.TypesInfo,
                typesInfo => typesInfo.TypeId == template.TypeId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == template.QuestId);

            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(_context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var amount = int.Parse(collection["Amount"]);
            var questionTexts = Enumerable.Repeat(template.QuestionText, amount).ToList();
            var questionAnswers = new string[amount][];

            for (var i = 0; i < amount; ++i)
            {
                questionAnswers[i] = new string[answers.Count];
                for (var j = 0; j < answers.Count; ++j)
                {
                    questionAnswers[i][j] = answers[j].Answer;
                }
            }

            var variables = (string) collection["Code"];

            var assembly = StorageUtils.CompileSourceRoslyn($"{_sourceCodePartOne}{variables}{_sourceCodePartTwo}");
            var type = assembly.GetType("QuestionGenerator");
            var instance = Activator.CreateInstance(type);

            type.InvokeMember("Generate",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null, instance, new object[] {questionTexts, questionAnswers, amount});

            for (var i = 0; i < amount; ++i)
            {
                //TODO: Change i + 1 to template questions count + 1
                var question = await QuestionExtensions.CreateQuestion(_context,
                    questionTexts[i], TypesExtensions.GetShortTypeName(template.TypeId),
                    $"{template.QuestionName}#{i + 1}",
                    new StringValues("off"));

                await StorageUtils.SaveToDatabase(_context, question);

                await AddResponseOptions(template.TypeId, question.QuestId,
                    questionAnswers[i], StorageUtils.GetResponseOptionsCorrectness(answers));
            }

            return RedirectToAction("Display", new {id});
        }

        private async Task AddResponseOption(int questionId, string text, bool isCorrect = true) =>
            await StorageUtils.SaveToDatabase(_context, await QuestionExtensions.CreateQuestionAnswerVariant(_context,
                questionId, text, isCorrect));

        private async Task AddResponseOptions<T, TV>(T typeInfo, int questionId, string[] responseOptions,
            TV[] correctnessInfo) where T : IConvertible
        {
            var questionTypeId = typeof(T) == typeof(string)
                ? TypesExtensions.GetTypeId(typeInfo)
                : Convert.ToInt32(typeInfo);

            var isCorrect = typeof(TV) == typeof(bool)
                ? Array.ConvertAll(correctnessInfo, value => Convert.ToBoolean(value))
                : StorageUtils.PreprocessCheckboxValues(
                    new StringValues(Array.ConvertAll(correctnessInfo, value => Convert.ToString(value))));

            if (questionTypeId == 3)
            {
                await AddResponseOption(questionId, responseOptions.First());
            }
            else
            {
                for (var i = 0; i < responseOptions.Length; ++i)
                {
                    await AddResponseOption(questionId, responseOptions[i], isCorrect[i]);
                }
            }
        }

        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;

            return View("Error");
        }

        private async Task<HashSet<TagsInfo>> GetTags(QuestionsInfo question)
        {
            await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);

            var currentTags = new HashSet<TagsInfo>();
            
            foreach (var tagQuestion in tagsQuestions)
            {
                currentTags.Add(await DataStorage.GetByPredicateAsync(_context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId));
            }

            return currentTags;
        }

        private async Task<int> GetVersion(int id)
        {
            var questions = await DataStorage.GetListByPredicateAsync(_context.QuestionsInfo,
                questionsInfo => questionsInfo.SourceQuestId == id);

            var versionId = 0;
            foreach (var q in questions.Where(q => q.VersionId > versionId))
            {
                versionId = q.VersionId;
            }

            return versionId + 1;
        }

        private HashSet<int> GetQuestionIdentifiers(List<TagsQuestions> tagsQuestions, StringValues amountValue)
        {
            var allQuestionIdentifiers = tagsQuestions
                .Select(tagsQuestions => tagsQuestions.QuestId)
                .ToHashSet();

            var amount = amountValue[0];

            var questionIdentifiers = new HashSet<int>();

            if (!string.IsNullOrWhiteSpace(amount))
            {
                var questionsAmount = int.Parse(amount);
                var count = 0;
                while (questionsAmount > 0)
                {
                    questionIdentifiers.Add(
                        allQuestionIdentifiers.ElementAt(Random.Next(allQuestionIdentifiers.Count)));
                    if (questionIdentifiers.Count > count)
                    {
                        --questionsAmount;
                    }
                }

                allQuestionIdentifiers = questionIdentifiers;
            }

            return allQuestionIdentifiers;
        }

        private async Task CreateNewTag(string tagName, int questionId)
        {
            var tag = await TagsExtensions.CreateTag(_context, tagName);

            await StorageUtils.SaveToDatabase(_context, tag);
            await StorageUtils.SaveToDatabase(_context, new TagsQuestions {TagId = tag.TagId, QuestId = questionId});
        }

        private async Task AddTagsToDatabase(StringValues tags, int questionId, QuestionsInfo question = null,
            bool isCreating = true)
        {
            if (question != null && question.IsTemplate)
            {
                await StorageUtils.SaveToDatabase(_context, new TagsQuestions {TagId = 1, QuestId = questionId});
            }

            // Adding tags info to database.
            foreach (var tagInfo in tags)
            {
                if (TagsExtensions.IsValidTagId(tagInfo))
                {
                    // if it is valid tag id
                    var exists = int.TryParse(tagInfo.TrimStart('ŧ'), out var tagId);
                    // if it is tag id from database
                    exists &= await _context.TagsInfo.AnyAsync(t => t.TagId == tagId);
                    if (!exists)
                    {
                        await CreateNewTag(tagInfo, questionId);
                    }
                    else if (isCreating || !_context.TagsQuestions
                        .Any(tq => tq.TagId == tagId && tq.QuestId == questionId))
                    {
                        await StorageUtils.SaveToDatabase(_context,
                            new TagsQuestions {TagId = tagId, QuestId = questionId});
                    }
                }
                else
                {
                    await CreateNewTag(tagInfo, questionId);
                }
            }
        }

        private const string QuestionText = "Question.QuestionText";
        private const string QuestionName = "Question.QuestionName";
        private const string TypeInfo = "Question.Type.Name";
        private const string AnswerText = "AnswerOption.Answer";

        private readonly string _sourceCodePartOne = @"
                using System;
                using System.Collections.Generic;
                using QuestionStorage.Utils;

                class QuestionGenerator
                {
                    private static QRandom rnd = new QRandom();

	                private static string ChangeTemplateFields(string text, ref string[] answers, Dictionary<string, object> list)
	                {	var newText = text;
		                foreach (var variable in list) {
                            newText = newText.Replace($" + '\u0022' + "${variable.Key}$" +
                                                     '\u0022' + ", $" + '\u0022' + "{variable.Value}" + '\u0022' + @");
                        }
                        for (int i = 0; i < answers.Length; ++i) {
                            foreach (var variable in list) {
                                answers[i] = answers[i].Replace($" + '\u0022' + "${variable.Key}$" +
                                                     '\u0022' + ", $" + '\u0022' + "{variable.Value}" + '\u0022' + @");
                            }
                        }

                        return newText;
                    }  
                    
                    public static void Generate(ref string[] text, ref string[][] answers, int amount)
                    {
                        for (var i = 0; i < amount; ++i) {
                            var d = new Dictionary<string, object>();

                        ";

        private readonly string _sourceCodePartTwo = @"    text[i] = ChangeTemplateFields(text[i], ref answers[i], d);
                        }
                    }
                }";
    }
}