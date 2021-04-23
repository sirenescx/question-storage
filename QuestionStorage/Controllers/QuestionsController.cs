using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models;
using QuestionStorage.Models.Options;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.Types;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Helpers;
using QuestionStorage.Helpers.Generation;
using QuestionStorage.Helpers.Xml;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly StorageContext context;
        private readonly IWebHostEnvironment environment;
        private readonly IOptionsMonitor<QuestionOptions> questionOptions;

        private static readonly Random Random = new Random();

        public QuestionsController(StorageContext context,
            IWebHostEnvironment environment, IOptionsMonitor<QuestionOptions> questionOptions)
        {
            this.context = context;
            this.environment = environment;
            this.questionOptions = questionOptions;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("ListQuestions", "Display");
        }

        [HttpGet]
        public async Task<ActionResult> Details(int courseId, int questionId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var question = await context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Type)
                .Include(q => q.TagsQuestions)
                .ThenInclude(tq => tq.Tag)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return ErrorPage(404);
            }

            if (question.CourseId != courseId)
            {
                return RedirectToAction("ListCourses", "Display");
            }

            (List<string> text, string code) = SplitTextAndCode(question.Text);
            ViewData["Text"] = text;
            ViewData["Code"] = $"<pre>{code}</pre>";

            return View(question);
        }

        [HttpGet]
        public async Task<ActionResult> Create(int courseId)
        {
            ViewData["Tags"] = new HashSet<Tag>(await context.Tags
                .Where(t => t.CourseId == courseId)
                .ToListAsync());

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int courseId, IFormCollection data)
        {
            try
            {
                var question = await CreateQuestion(courseId, data);
                return RedirectToAction("Details", new {courseId = question.CourseId, questionId = question.Id});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int courseId, int questionId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var data = await PullData(questionId, ViewData);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int courseId, int questionId, IFormCollection data)
        {
            try
            {
                var question = await context.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
                int sourceQuestionId = await GetSourceVersionId(context, question);
                var parsedResult = await ParseQuestionData(questionOptions.CurrentValue, data, courseId);
                var newQuestion = await CreateAndSaveQuestion(
                    parsedResult,
                    await GetVersion(sourceQuestionId),
                    sourceQuestionId);
                await CreateAndSaveAnswerOptions(newQuestion.Id,
                    data[questionOptions.CurrentValue.ClientAnswerOptionName].ToArray(),
                    data[nameof(AnswerOption.IsCorrect)].ToArray());
                await CreateAndSaveTags(courseId, data[questionOptions.CurrentValue.ClientTagsName], newQuestion.Id);

                return RedirectToAction("Details", new {questionId = newQuestion.Id, courseId = newQuestion.CourseId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Copy(int courseId, int questionId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            return View(await PullData(questionId, ViewData));
        }

        [HttpPost]
        public async Task<ActionResult> Copy(int courseId, int questionId, IFormCollection data)
        {
            try
            {
                return RedirectToAction("Details",
                    new {questionId = (await CreateQuestion(courseId, data)).Id, courseId});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        public async Task<IActionResult> ExportToXml(int questionId)
        {
            var question = await context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question.Xml != null)
            {
                return File(Encoding.UTF8.GetBytes(question.Xml),
                    "application/xml", $"question{questionId}.xml");
            }

            //TODO:
            var document = XmlGenerator.ExportToXml(question, question.AnswerOptions.ToList(), environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", $"question{questionId}.xml");
        }

        public IActionResult Display(int courseId, int tagId) =>
            RedirectToAction("ListQuestionsByTag", "Display", new {courseId, tagId});

        [HttpGet]
        public async Task<IActionResult> Export(int courseId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            ViewData["Tags"] = new HashSet<Tag>(await context.Tags
                .Where(t => t.CourseId == courseId)
                .ToListAsync());

            return View();
        }

        //TODO
        [HttpPost]
        public async Task<IActionResult> Export(int courseId, IFormCollection data)
        {
            ModelState.Clear();

            ViewData["Tags"] = new HashSet<Tag>(await context.Tags
                .Where(t => t.CourseId == courseId)
                .ToListAsync());

            if (!data["Tags"].Any())
            {
                return View();
            }

            var tagIdentifiers = data["Tags"].Select(int.Parse).ToHashSet();
            var tagsQuestions = await context.TagsQuestions
                .Where(tq => tagIdentifiers.Contains(tq.TagId))
                .ToListAsync();
            var questionIdentifiers = GetQuestionIdentifiers(tagsQuestions, data["QuestionsAmount"]);

            if (questionIdentifiers is null)
            {
                ViewData["Error"] = "Error";
                return View();
            }

            var questions = await context.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIdentifiers.Contains(q.Id))
                .ToListAsync();

            var responseOptions = questions.Select(question => question.AnswerOptions.ToList()).ToList();

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
                string xmlData = await file.ReadAsStringAsync();
                var document = new XmlDocument();
                document.LoadXml(xmlData);

                var parsedResult = await ParseQuestionData(
                    XmlGenerator.GetElementTextFromXml(document, "questiontext"),
                    GetTypeIdFromFullName(XmlGenerator.FindQuestionType(document)),
                    XmlGenerator.GetElementTextFromXml(document, "name"),
                    courseId,
                    xmlData
                );

                var question = await CreateAndSaveQuestion(parsedResult);

                if (question.TypeId != 4)
                {
                    var (responseOptions, correct) = XmlGenerator.GetResponseInfo(document);
                    await CreateAndSaveAnswerOptions(question.Id, responseOptions, correct);
                }

                return RedirectToAction("Details", new {courseId, questionId = question.Id});
            }
            catch (XmlException ex)
            {
                ModelState.AddModelError("File", ex.Message);
                return View("Import");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int courseId, int questionId)
        {
            var question = await context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Type)
                .Include(q => q.TagsQuestions)
                .ThenInclude(tq => tq.Tag)
                .Where(q => q.Id == questionId)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return ErrorPage(404);
            }

            var model = new TemplateQuestionViewModel {Question = question};

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(int questionId, int courseId, IFormCollection data)
        {
            var template = await context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Type)
                .Include(q => q.TagsQuestions)
                .ThenInclude(tq => tq.Tag)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (template == null)
            {
                return ErrorPage(404);
            }

            int amount = int.Parse(data["Amount"]);
            string[] questionTexts = Enumerable.Repeat(template.Text, amount).ToArray();
            var questionAnswers = new string[amount][];
            var answers = template.AnswerOptions.ToList();

            for (var i = 0; i < amount; ++i)
            {
                questionAnswers[i] = new string[answers.Count];
                for (var j = 0; j < answers.Count; ++j)
                {
                    questionAnswers[i][j] = answers[j].Text;
                }
            }

            var variables = (string) data["Code"];

            var assembly =
                AssemblyBuilder.CompileSourceRoslyn(XmlGenerator.GetCode(variables, environment.WebRootPath));
            var type = assembly.GetType("QuestionGenerator");
            var instance = Activator.CreateInstance(type);

            type.InvokeMember("Generate",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null, instance, new object[] {questionTexts, questionAnswers, amount});

            var questionIdentifiers = new List<int>();

            for (var i = 0; i < amount; ++i)
            {
                var parsedResult = await ParseQuestionData(
                    questionTexts[i],
                    GetShortTypeName(template.TypeId),
                    $"{i + 1}",
                    courseId
                );
                var question = await CreateAndSaveQuestion(parsedResult);

                await CreateAndSaveAnswerOptions(question.Id, questionAnswers[i],
                    GetResponseOptionsCorrectness(answers));

                questionIdentifiers.Add(question.Id);
            }

            return RedirectToAction("List", "Display",
                new {courseId, questions = string.Join('&', questionIdentifiers)});
        }

        #region Helper Functions

        private const string CodeOpeningTag = "<code>";
        private const string CodeClosingTag = "</code>";

        private static AnswerOption CreateAnswerOption(int questionId, string text, bool isCorrect = true) =>
            new AnswerOption
            {
                QuestionId = questionId,
                Text = text,
                IsCorrect = isCorrect
            };

        private async Task AddResponseOption(int questionId, string text, bool isCorrect = true) =>
            await Helpers.Common.SaveToDatabase(context, CreateAnswerOption(questionId, text, isCorrect));

        private async Task CreateAndSaveAnswerOptions<T>(int questionId, string[] responseOptions, T[] correctness)
        {
            if (responseOptions.Length == 1)
            {
                await AddResponseOption(questionId, responseOptions.First());
            }
            else
            {
                bool[] isCorrect = typeof(T) == typeof(bool)
                    ? Array.ConvertAll(correctness, value => Convert.ToBoolean(value))
                    : Helpers.Common.PreprocessCheckboxValues(
                        new StringValues(Array.ConvertAll(correctness, value => Convert.ToString(value))));

                for (var i = 0; i < responseOptions.Length; i++)
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

        private static int GetTypeId(string typeInfo) =>
            typeInfo.Equals("sc") ? 1 : typeInfo.Equals("mc") ? 2 : typeInfo.Equals("oa") ? 3 : 4;

        private static string GetTypeIdFromFullName(string typeInfo) =>
            typeInfo.Equals("multichoice") ? "sc" :
            typeInfo.Equals("multichoiceset") ? "mc" :
            typeInfo.Equals("shortanswer") ? "oa" : "o";

        private static string GetShortTypeName(int typeId) =>
            typeId == 1 ? "sc" : typeId == 2 ? "mc" : typeId == 3 ? "oa" : "o";

        private async Task<HashSet<Tag>> GetTags(Question question)
        {
            var currentTags = new HashSet<Tag>();

            foreach (var tagQuestion in question.TagsQuestions)
            {
                currentTags.Add(await context.Tags.FirstOrDefaultAsync(t => t.Id == tagQuestion.TagId));
            }

            return currentTags;
        }

        private async Task<int> GetVersion(int id) =>
            (await context.Questions.Where(q => q.SourceId == id).ToListAsync())
            .Max(q => q.VersionId) + 1;

        private HashSet<int> GetQuestionIdentifiers(List<TagsQuestions> tagsQuestions, StringValues amountValue)
        {
            var questionIdentifiers = tagsQuestions
                .Select(tq => tq.QuestionId)
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

        private async Task<Question> CreateAndSaveQuestion(
            QuestionDto questionDto,
            int versionId = 1,
            int sourceId = -1,
            string xml = null)
        {
            var question = new Question
            {
                Name = questionDto.Name,
                Text = questionDto.Text,
                TypeId = questionDto.TypeId,
                IsTemplate = questionDto.IsTemplate,
                AuthorId = questionDto.AuthorId,
                CourseId = questionDto.CourseId,
                VersionId = versionId
            };

            question = (await context.AddAsync(question)).Entity;
            await context.SaveChangesAsync();

            question.SourceId = sourceId == -1 ? question.Id : sourceId;
            await context.SaveChangesAsync();

            return question;
        }

        private async Task<QuestionDto> ParseQuestionData(QuestionOptions options, IFormCollection data, int courseId)
        {
            string templateKey = nameof(Question.IsTemplate);
            var isTemplate = data.ContainsKey(templateKey) ? data[nameof(Question.IsTemplate)] : new StringValues();

            return new QuestionDto
            {
                TypeId = GetTypeId(data[options.ClientTypeName].First()),
                Name = data[nameof(Question.Name)].First().Trim(),
                Text = data[nameof(Question.Text)].First().Trim(),
                AuthorId = await Helpers.Common.GetUserId(context, User.Identity.Name),
                IsTemplate = isTemplate.Any() && Helpers.Common.PreprocessCheckboxValues(isTemplate).First(),
                CourseId = courseId
            };
        }

        private async Task<QuestionDto> ParseQuestionData(string text, string type, string name, int courseId,
            string xml = null) =>
            new QuestionDto
            {
                TypeId = GetTypeId(type),
                Name = name.Trim(),
                Text = text.Trim(),
                AuthorId = await Helpers.Common.GetUserId(context, User.Identity.Name),
                CourseId = courseId,
                Xml = xml
            };

        private async Task<Question> CreateQuestion(int courseId, IFormCollection data)
        {
            var currentQuestionOptions = questionOptions.CurrentValue;
            var parsedResult = await ParseQuestionData(currentQuestionOptions, data, courseId);
            var question = await CreateAndSaveQuestion(parsedResult);
            await CreateAndSaveAnswerOptions(
                question.Id,
                data[currentQuestionOptions.ClientAnswerOptionName].ToArray(),
                data[nameof(AnswerOption.IsCorrect)].ToArray());
            await CreateAndSaveTags(courseId, data["Tags"], question.Id, question);

            return question;
        }

        private static async Task<int> GetSourceVersionId(StorageContext context, Question editableQuestion)
        {
            while (editableQuestion.SourceId != editableQuestion.Id)
            {
                var question = editableQuestion;
                editableQuestion =
                    await context.Questions.Where(
                        q => q.Id == question.SourceId).FirstOrDefaultAsync();
            }

            return editableQuestion.Id;
        }

        private static async Task<Tag> CreateTag(string name, int courseId, int? parentId = null) =>
            new Tag
            {
                Name = name.Trim(),
                CourseId = courseId,
                ParentId = parentId
            };

        private static bool IsValidTagId(string tagInfo) => tagInfo.ElementAt(0).Equals('ŧ');

        private async Task CreateNewTag(string tagName, int questionId, int courseId)
        {
            var tag = await CreateTag(tagName, courseId);

            await Helpers.Common.SaveToDatabase(context, tag);
            await Helpers.Common.SaveToDatabase(context, new TagsQuestions {TagId = tag.Id, QuestionId = questionId});
        }

        private async Task CreateAndSaveTags(int courseId, StringValues tags, int questionId,
            Question question = null,
            bool isCreating = true)
        {
            foreach (var tag in tags)
            {
                if (IsValidTagId(tag))
                {
                    var exists = int.TryParse(tag.TrimStart('ŧ'), out var tagId);
                    exists &= await context.Tags.AnyAsync(t => t.Id == tagId);
                    if (!exists)
                    {
                        await CreateNewTag(tag, questionId, courseId);
                    }
                    else if (isCreating || !context.TagsQuestions
                        .Any(tq => tq.TagId == tagId && tq.QuestionId == questionId))
                    {
                        await Helpers.Common.SaveToDatabase(context,
                            new TagsQuestions {TagId = tagId, QuestionId = questionId});
                    }
                }
                else
                {
                    await CreateNewTag(tag, questionId, courseId);
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

        private async Task<Question> PullData(int id, ViewDataDictionary viewData)
        {
            void RedirectToErrorPage() => ErrorPage(404);

            var question = await context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.TagsQuestions)
                .ThenInclude(tq => tq.Tag)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                RedirectToErrorPage();
            }

            viewData["CurrentTags"] = await GetTags(question);
            viewData["AllTags"] = new HashSet<Tag>(await context.Tags.ToListAsync());

            return question;
        }

        public async Task<IActionResult> AddComment(int courseId, int questionId, IFormCollection data)
        {
            var question = await context.Questions.FirstOrDefaultAsync(q => q.Id.Equals(questionId));
            question.Comment = data[nameof(Question.Comment)];
            await context.SaveChangesAsync();

            return RedirectToAction("Details", new {courseId, questionId});
        }

        private static bool[] GetResponseOptionsCorrectness(List<AnswerOption> answers)
        {
            var correct = new bool[answers.Count];
            for (var i = 0; i < answers.Count; ++i)
            {
                correct[i] = answers[i].IsCorrect;
            }

            return correct;
        }

        #endregion
    }
}