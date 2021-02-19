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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private readonly StorageContext context;
        private readonly IWebHostEnvironment environment;

        private static readonly Random Random = new Random();

        public QuestionsController(StorageContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("ListQuestions", "Display");
        }

        [HttpGet]
        public async Task<ActionResult> Details(int courseId, int questionId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }
            
            var question = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);
            
            if (question == null)
            {
                return ErrorPage(404);
            }

            if (question.CourseId != courseId)
            {
                return RedirectToAction("ListCourses", "Display");
            }

            await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            await DataStorage.GetByPredicateAsync(context.TypesInfo,
                typesInfo => typesInfo.TypeId == question.TypeId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);

            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var (text, code) = SplitTextAndCode(question.QuestionText);
            ViewData["Text"] = text;
            ViewData["Code"] = $"<pre>{code}</pre>";

            return View(question);
        }

        [HttpGet]
        public async Task<ActionResult> Create(int courseId)
        {
            ViewData["Tags"] = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(context.TagsInfo,
                tagsInfo => tagsInfo.CourseId == courseId,
                tagsInfo => tagsInfo);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public async Task<ActionResult> Create(int courseId, IFormCollection collection)
        {
            try
            {
                var question = await CreateQuestion(courseId, collection);
                return RedirectToAction("Details",
                    new {courseId = question.CourseId, questionId = question.QuestId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int courseId, int questionId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            return View(await PullData(questionId, ViewData));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int courseId, int questionId, IFormCollection collection)
        {
            try
            {
                var typeName = collection["Question.Type.Name"][0];

                var question = await DataStorage.GetByPredicateAsync(
                    context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);

                var sourceQuestionId = await QuestionExtensions.GetSourceVersionId(context, question);

                var newQuestion = await QuestionExtensions.CreateQuestion(context,
                    collection["QuestionName"], collection["QuestionText"],
                    typeName, await StorageUtils.GetUserId(context, User.Identity.Name),
                    question.CourseId, await GetVersion(sourceQuestionId), sourceQuestionId);

                await StorageUtils.SaveToDatabase(context, newQuestion);

                await AddTagsToDatabase(courseId, collection["Tags"], newQuestion.QuestId);

                await AddResponseOptions(typeName, newQuestion.QuestId,
                    collection["AnswerOption.Answer"].ToArray(),
                    collection["Correct"].ToArray());

                return RedirectToAction("Details",
                    new {questionId = newQuestion.QuestId, courseId = newQuestion.CourseId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Copy(int courseId, int questionId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            return View(await PullData(questionId, ViewData));
        }

        [HttpPost]
        public async Task<ActionResult> Copy(int courseId, int questionId, IFormCollection collection)
        {
            try
            {
                return RedirectToAction("Details",
                    new {questionId = (await CreateQuestion(courseId, collection)).QuestId, courseId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        public async Task<IActionResult> ExportToXml(int questionId)
        {
            var question = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);

            if (question.QuestionXml != null)
            {
                return File(Encoding.UTF8.GetBytes(question.QuestionXml),
                    "application/xml", $"question{questionId}.xml");
            }

            var responseOptions = await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == questionId);

            var document = XmlGenerator.ExportToXml(question, responseOptions, environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", $"question{questionId}.xml");
        }

        public IActionResult Display(int courseId, int tagId) =>
            RedirectToAction("ListQuestionsByTag", "Display", new {courseId, tagId});

        [HttpGet]
        public async Task<IActionResult> Export(int courseId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            ViewData["Tags"] = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(context.TagsInfo,
                tagsInfo => tagsInfo.CourseId == courseId,
                tagsInfo => tagsInfo);

            return View();
        }

        //TODO
        [HttpPost]
        public async Task<IActionResult> Export(int courseId, IFormCollection collection)
        {
            ModelState.Clear();

            ViewData["Tags"] = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(context.TagsInfo,
                tagsInfo => tagsInfo.CourseId == courseId,
                tagsInfo => tagsInfo);

            if (!collection["Tags"].Any())
            {
                return View();
            }

            var tagIdentifiers = collection["Tags"].Select(int.Parse).ToHashSet();

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagIdentifiers.Contains(tagsQuestions.TagId));

            var questionIdentifiers = GetQuestionIdentifiers(tagsQuestions, collection["QuestionsAmount"]);

            if (questionIdentifiers is null)
            {
                ViewData["Error"] = "Error";
                return View();
            }

            var questions = await DataStorage.GetListByPredicateAsync(context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId));

            var responseOptions = new List<List<QuestionAnswerVariants>>();

            foreach (var questionId in questionIdentifiers)
            {
                responseOptions.Add(await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                    questionAnswerVariants => questionAnswerVariants.QuestId == questionId));
            }

            var document = XmlGenerator.ExportQuestionsToXml(questions, responseOptions, environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", "questions.xml");
        }


        //TODO
        [HttpGet]
        public IActionResult Import(int courseId)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(int courseId, IFormFile file)
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

                var type = new StringValues(
                    TypesExtensions.GetTypeIdFromFullName(XmlGenerator.FindQuestionType(document)));

                var question = await QuestionExtensions.CreateQuestion(context,
                    XmlGenerator.GetElementTextFromXml(document, "questiontext"), type,
                    XmlGenerator.GetElementTextFromXml(document, "name"),
                    await StorageUtils.GetUserId(context, User.Identity.Name), new StringValues(), courseId, xmlData);

                await StorageUtils.SaveToDatabase(context, question);

                if (question.TypeId != 4)
                {
                    var (responseOptions, correct) = XmlGenerator.GetResponseInfo(document);
                    await AddResponseOptions(TypesExtensions.GetTypeId(type), question.QuestId, responseOptions,
                        correct);
                }

                return RedirectToAction("Details", new {courseId, questionId = question.QuestId});
            }
            catch (XmlException ex)
            {
                ModelState.AddModelError("File", ex.Message);
                return View("Import");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int questionId, int courseId)
        {
            var question = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await DataStorage.GetByPredicateAsync(context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            await DataStorage.GetByPredicateAsync(context.TypesInfo,
                typesInfo => typesInfo.TypeId == question.TypeId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);

            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var model = new TemplateQuestionViewModel {Question = question};

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(int questionId, int courseId, IFormCollection collection)
        {
            var template = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);

            if (template == null)
            {
                return ErrorPage(404);
            }

            var answers = await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == template.QuestId);

            await DataStorage.GetByPredicateAsync(context.TypesInfo,
                typesInfo => typesInfo.TypeId == template.TypeId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == template.QuestId);

            foreach (var tagQuestion in tagsQuestions)
            {
                await DataStorage.GetListByPredicateAsync(context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
            }

            var amount = int.Parse(collection["Amount"]);
            var questionTexts = Enumerable.Repeat(template.QuestionText, amount).ToArray();
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

            var assembly = StorageUtils.CompileSourceRoslyn(XmlGenerator.GetCode(variables, environment.WebRootPath));
            var type = assembly.GetType("QuestionGenerator");
            var instance = Activator.CreateInstance(type);

            type.InvokeMember("Generate",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null, instance, new object[] {questionTexts, questionAnswers, amount});

            var questionIdentifiers = new List<int>();

            for (var i = 0; i < amount; ++i)
            {
                var question = await QuestionExtensions.CreateQuestion(context,
                    questionTexts[i], TypesExtensions.GetShortTypeName(template.TypeId),
                    $"{i + 1}",
                    await StorageUtils.GetUserId(context, User.Identity.Name),
                    new StringValues("off"), courseId);

                await StorageUtils.SaveToDatabase(context, question);

                await AddResponseOptions(template.TypeId, question.QuestId,
                    questionAnswers[i], StorageUtils.GetResponseOptionsCorrectness(answers));

                questionIdentifiers.Add(question.QuestId);
            }

            return RedirectToAction("List", "Display",
                new {courseId, questions = string.Join('&', questionIdentifiers)});
        }

        private async Task AddResponseOption(int questionId, string text, bool isCorrect = true) =>
            await StorageUtils.SaveToDatabase(context, await QuestionExtensions.CreateQuestionAnswerVariant(context,
                questionId, text, isCorrect));

        private async Task AddResponseOptions<T, TV>(T typeInfo, int questionId, string[] responseOptions,
            TV[] correctnessInfo) where T : IConvertible
        {
            var questionTypeId = typeof(T) == typeof(string)
                ? TypesExtensions.GetTypeId(typeInfo)
                : Convert.ToInt32(typeInfo);

            var isCorrect = typeof(TV) == typeof(bool)
                ? Array.ConvertAll(correctnessInfo, value => Convert.ToBoolean(value))
                : questionTypeId != 3
                    ? StorageUtils.PreprocessCheckboxValues(
                        new StringValues(Array.ConvertAll(correctnessInfo, value => Convert.ToString(value))))
                    : new[] {true};

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
            await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                questionAnswerVariants => questionAnswerVariants.QuestId == question.QuestId);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagsQuestions.QuestId == question.QuestId);

            var currentTags = new HashSet<TagsInfo>();

            foreach (var tagQuestion in tagsQuestions)
            {
                currentTags.Add(await DataStorage.GetByPredicateAsync(context.TagsInfo,
                    tagsInfo => tagsInfo.TagId == tagQuestion.TagId));
            }

            return currentTags;
        }

        private async Task<int> GetVersion(int id) =>
            (await DataStorage.GetListByPredicateAsync(context.QuestionsInfo,
                questionsInfo => questionsInfo.SourceQuestId == id))
            .Max(question => question.VersionId) + 1;

        private HashSet<int> GetQuestionIdentifiers(List<TagsQuestions> tagsQuestions, StringValues amountValue)
        {
            var questionIdentifiers = tagsQuestions
                .Select(tagsQuestions => tagsQuestions.QuestId)
                .ToHashSet();

            var amount = amountValue[0];

            if (string.IsNullOrWhiteSpace(amount))
            {
                return questionIdentifiers;
            }

            var questionsAmount = int.Parse(amount);

            if (questionIdentifiers.Count < questionsAmount)
            {
                return null;
            }

            var questionIdentifiersHelper = new HashSet<int>();

            while (questionsAmount != questionIdentifiersHelper.Count)
            {
                questionIdentifiersHelper.Add(
                    questionIdentifiers.ElementAt(Random.Next(questionIdentifiers.Count)));
            }

            questionIdentifiers = questionIdentifiersHelper;

            return questionIdentifiers;
        }

        private async Task<QuestionsInfo> PullData(int id, ViewDataDictionary viewData)
        {
            void RedirectToErrorPage() => ErrorPage(404);

            var question = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == id);

            if (question == null)
            {
                RedirectToErrorPage();
            }

            viewData["CurrentTags"] = await GetTags(question);
            viewData["AllTags"] = await DataStorage.GetHashSetAsync(context.TagsInfo);

            return question;
        }

        private async Task<QuestionsInfo> CreateQuestion(int courseId, IFormCollection collection)
        {
            var question = await QuestionExtensions.CreateQuestion(context,
                collection["QuestionText"], collection["Question.Type.Name"],
                collection["QuestionName"], await StorageUtils.GetUserId(context, User.Identity.Name),
                collection["IsTemplate"], courseId);

            await StorageUtils.SaveToDatabase(context, question);

            await AddResponseOptions(collection["Question.Type.Name"][0], question.QuestId,
                collection["AnswerOption.Answer"].ToArray(),
                collection["Correct"].ToArray());

            await AddTagsToDatabase(courseId, collection["Tags"], question.QuestId, question);

            return question;
        }

        private async Task CreateNewTag(string tagName, int questionId, int courseId)
        {
            var tag = await TagsExtensions.CreateTag(context, tagName, courseId);

            await StorageUtils.SaveToDatabase(context, tag);
            await StorageUtils.SaveToDatabase(context, new TagsQuestions {TagId = tag.TagId, QuestId = questionId});
        }

        private async Task AddTagsToDatabase(int courseId, StringValues tags, int questionId,
            QuestionsInfo question = null,
            bool isCreating = true)
        {
            // if (question != null && question.IsTemplate)
            // {
            //     await StorageUtils.SaveToDatabase(context, new TagsQuestions {TagId = 1, QuestId = questionId});
            // }

            foreach (var tagInfo in tags)
            {
                if (TagsExtensions.IsValidTagId(tagInfo))
                {
                    var exists = int.TryParse(tagInfo.TrimStart('ŧ'), out var tagId);
                    exists &= await context.TagsInfo.AnyAsync(t => t.TagId == tagId);
                    if (!exists)
                    {
                        await CreateNewTag(tagInfo, questionId, courseId);
                    }
                    else if (isCreating || !context.TagsQuestions
                        .Any(tq => tq.TagId == tagId && tq.QuestId == questionId))
                    {
                        await StorageUtils.SaveToDatabase(context,
                            new TagsQuestions {TagId = tagId, QuestId = questionId});
                    }
                }
                else
                {
                    await CreateNewTag(tagInfo, questionId, courseId);
                }
            }
        }

        private static (List<string>, string) SplitTextAndCode(string questionText)
        {
            var codeStartIndex = questionText.IndexOf(CodeOpeningTag, StringComparison.Ordinal);
            var codeEndIndex = questionText.IndexOf(CodeClosingTag, StringComparison.Ordinal);
            
            if (codeStartIndex == -1 || codeEndIndex == -1)
            {
                return (new List<string> {questionText}, null);
            }
            
            var task = new List<string> {questionText.Substring(0, codeStartIndex)};
            var code = questionText.Substring(codeStartIndex, codeEndIndex - codeStartIndex + CodeClosingTag.Length);
            task.Add(questionText.Substring(codeEndIndex + CodeClosingTag.Length));

            return (task, code);
        }

        private const string CodeOpeningTag = "<code>";
        private const string CodeClosingTag = "</code>";

        public async Task<IActionResult> AddComment(int courseId, int questionId, IFormCollection collection)
        {
            var question = await DataStorage.GetByPredicateAsync(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId == questionId);
            question.Comment = collection["Comment"];
            await context.SaveChangesAsync();
            
            return RedirectToAction("Details", new {courseId, questionId});
        }
    }
}