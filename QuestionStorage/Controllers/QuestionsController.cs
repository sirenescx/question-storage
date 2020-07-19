using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Models.QuizzesQuestionsModels;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Utils;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly HSE_QuestContext _context;

        public QuestionsController(HSE_QuestContext context)
        {
            _context = context;
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
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(m => m.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await _context.QuestionAnswerVariants
                .Where(a => a.QuestId == question.QuestId).ToListAsync();
            await _context.TypesInfo.FirstOrDefaultAsync(t => t.TypeId == question.TypeId);
            var tagsQuestions = await _context.TagsQuestions
                .Where(tq => tq.QuestId == question.QuestId).ToListAsync();
            foreach (var tagQuestion in tagsQuestions)
            {
                await _context.TagsInfo
                    .Where(t => t.TagId == tagQuestion.TagId).ToListAsync();
            }

            return View(question);
        }

        // GET: Questions/Create
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewData["Tags"] = new HashSet<TagsInfo>(await _context.TagsInfo.ToListAsync());

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
                // Adding question info to database.
                var questionId = await _context.QuestionsInfo.MaxAsync(q => q.QuestId) + 1;
                var question = StorageUtils.CreateQuestion(
                    collection[QuestionText], questionId, StorageUtils.GetTypeId(collection[TypeInfo]), 
                    collection[QuestionName], collection["IsTemplate"]);

                await SaveToDatabase(question);

                await AddResponseOptions(collection[TypeInfo], questionId, collection[AnswerText],
                    collection["Correct"]);

                await AddTagsToDatabase(collection["Tags"], questionId, question);


                return RedirectToAction("Details", new {id = questionId});
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
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(m => m.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await _context.QuestionAnswerVariants.Where(
                a => a.QuestId == question.QuestId).ToListAsync();
            var tagsQuestions = await _context.TagsQuestions.Where(
                qt => qt.QuestId == question.QuestId).ToListAsync();
            var currentTags = new HashSet<TagsInfo>();
            foreach (var tagQuestion in tagsQuestions)
            {
                var tagInfo = await _context.TagsInfo.FirstOrDefaultAsync(
                    t => t.TagId == tagQuestion.TagId);
                currentTags.Add(tagInfo);
            }

            ViewData["CurrentTags"] = currentTags;
            ViewData["AllTags"] = _context.TagsInfo.ToHashSet();

            return View(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                var tags = collection["Tags"];
                var responseOptions = collection[AnswerText];
                var typeName = collection["Type.Name"];
                var changed = await DeleteAllExistingResponseOptions(id, responseOptions);

                await EditQuestion(id, collection["QuestionName"],
                    collection["QuestionText"], typeName);
                await DeleteUnusedTags(id, tags);
                await AddTagsToDatabase(tags, id, isCreating: false);

                // if (changed)
                // {
                await AddResponseOptions(typeName, id, collection[AnswerText],
                    collection["Correct"]);
                // }

                return RedirectToAction("Details", new {id});
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        public IActionResult ExportToPdf(int id)
        {
            MemoryStream ms;
            using (ms = new MemoryStream())
            {
                using var client = new WebClient();
                var url = $"https://localhost:5001/Questions/Details/{id}";
                var htmlString = client.DownloadString(url);
                using var pdfWriter = new PdfWriter(ms);
                pdfWriter.SetCloseStream(false);
                using var document = HtmlConverter.ConvertToDocument(htmlString, pdfWriter);
            }

            return File(ms.ToArray(), "application/pdf", $"question{id}.pdf");
        }

        public async Task<IActionResult> ExportToXml(int id)
        {
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(q => q.QuestId == id);

            if (question.QuestionXml != null)
            {
                return File(Encoding.UTF8.GetBytes(question.QuestionXml),
                    "application/xml", $"question{id}.xml");
            }

            var responseOptions = await _context.QuestionAnswerVariants
                .Where(qav => qav.QuestId == id).ToListAsync();
            var document = XmlGenerator.ExportToXml(question, responseOptions);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", $"question{id}.xml");
        }

        public IActionResult Display(int id)
        {
            return RedirectToAction("ListByTag", "Display", new {id});
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            ViewData["Tags"] = new HashSet<TagsInfo>(await _context.TagsInfo.ToListAsync());

            return View();
        }

        public async Task<IActionResult> Export(IFormCollection collection)
        {
            ViewData["Tags"] = new HashSet<TagsInfo>(await _context.TagsInfo.ToListAsync());
            if (collection["Tags"].Count == 0)
            {
                return View();
            }

            var tagsIds = collection["Tags"].Select(int.Parse).ToHashSet();
            var tagsQuestions = await _context.TagsQuestions
                .Where(qt => tagsIds.Contains(qt.TagId)).ToListAsync();
            var questionsIds = tagsQuestions.Select(tq => tq.QuestId).ToHashSet();
            var questions = await _context.QuestionsInfo
                .Where(q => questionsIds.Contains(q.QuestId)).ToListAsync();
            var responseOptions = new List<List<QuestionAnswerVariants>>();
            foreach (var questionId in questionsIds)
            {
                responseOptions.Add(await _context.QuestionAnswerVariants
                    .Where(qav => qav.QuestId == questionId).ToListAsync());
            }

            var document = XmlGenerator.ExportQuestionsToXml(questions, responseOptions);

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

                var type = StorageUtils.GetTypeIdFromFullName(XmlGenerator.FindQuestionType(document));
                var questionId = await _context.QuestionsInfo.MaxAsync(q => q.QuestId) + 1;
                var question = StorageUtils.CreateQuestion(
                    XmlGenerator.GetElementTextFromXml(document, "questiontext"), questionId, 
                    StorageUtils.GetTypeId(type), 
                    XmlGenerator.GetElementTextFromXml(document, "name"), xmlData);

                await SaveToDatabase(question);

                if (question.TypeId != 4)
                {
                    var (responseOptions, correct) = XmlGenerator.GetResponseInfo(document);
                    await AddResponseOptions(StorageUtils.GetTypeId(type), questionId, responseOptions, correct);
                }

                return RedirectToAction("Details", new {id = questionId});
            }
            catch (XmlException ex)
            {
                ModelState.AddModelError("File", ex.Message);
                return View("Import");
            }

            // catch
            // {
            //     return ErrorPage(400);
            // }
        }
        
        [HttpGet]
        public async Task<IActionResult> Generate(int id)
        {
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(m => m.QuestId == id);

            if (question == null)
            {
                return ErrorPage(404);
            }

            await _context.QuestionAnswerVariants
                .Where(a => a.QuestId == question.QuestId).ToListAsync();
            await _context.TypesInfo.FirstOrDefaultAsync(t => t.TypeId == question.TypeId);
            var tagsQuestions = await _context.TagsQuestions
                .Where(tq => tq.QuestId == question.QuestId).ToListAsync();
            foreach (var tagQuestion in tagsQuestions)
            {
                await _context.TagsInfo
                    .Where(t => t.TagId == tagQuestion.TagId).ToListAsync();
            }

            var model = new TemplateQuestionViewModel {Question = question};

            return View(model);
        }
        
        public async Task<IActionResult> Generate(IFormCollection collection, int id)
        {
            var template = await _context.QuestionsInfo
                .FirstOrDefaultAsync(m => m.QuestId == id);

            if (template == null)
            {
                return ErrorPage(404);
            }

            var answers = await _context.QuestionAnswerVariants
                .Where(a => a.QuestId == template.QuestId).ToListAsync();
            await _context.TypesInfo.FirstOrDefaultAsync(t => t.TypeId == template.TypeId);
            var tagsQuestions = await _context.TagsQuestions
                .Where(tq => tq.QuestId == template.QuestId).ToListAsync();
            foreach (var tagQuestion in tagsQuestions)
            {
                await _context.TagsInfo
                    .Where(t => t.TagId == tagQuestion.TagId).ToListAsync();
            }
            
            var amountOfQuestions = int.Parse(collection["Amount"]);
            var variables = (string) collection["Code"];

            for (var i = 0; i < amountOfQuestions; ++i)
            {
                var questionId = await _context.QuestionsInfo.MaxAsync(q => q.QuestId) + 1;
                var questionText = $"$@\"{StorageUtils.GetInterpolatedString(template.QuestionText)}\"";
                var responseOptionsText = StorageUtils.CreateStringArrayFromResponseOptions(answers);
                var correct = StorageUtils.GetResponseOptionsCorrectness(answers);

                var sourceCode = $@"
                using System;
                using System.Collections.Generic;

                class QuestionGenerator
                {{
                    public static Random rnd = new Random();

                    public static (string, string[]) Generate()
                    {{
                        {variables}

                        return ({questionText}, {responseOptionsText});
                    }}
                }}";
                var assembly = StorageUtils.CompileSourceRoslyn(sourceCode);
                var type = assembly.GetType("QuestionGenerator");
                var instance = Activator.CreateInstance(type);
                
                string[] responseOptions;
                (questionText, responseOptions) = ((string, string[])) type.InvokeMember("Generate",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null, instance, null);

                var question = StorageUtils.CreateQuestion(
                    questionText, questionId, template.TypeId, $"{template.QuestionName}#{i + 1}", 
                    new StringValues("off"));
                
                await SaveToDatabase(question);
                
                await AddResponseOptions(
                    template.TypeId, questionId, new List<string>(responseOptions), correct);
            }

            return RedirectToAction("Display", new {id = 11});
        }

        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;

            return View("Error");
        }

        private async Task SaveToDatabase<T>(T item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        private async Task AddResponseOption(int questionId, string text, bool isCorrect = true)
        {
            var variantId = await _context.QuestionAnswerVariants
                .MaxAsync(v => v.VariantId) + 1;
            var answerVariant = StorageUtils.CreateAnswerVariant(variantId, questionId, text, isCorrect);

            await SaveToDatabase(answerVariant);
        }

        private async Task AddResponseOptions(
            string questionTypeName, int questionId, StringValues responseOptions, StringValues correct)
        {
            var questionTypeId = StorageUtils.GetTypeId(questionTypeName);

            if (questionTypeId == 3)
            {
                await AddResponseOption(questionId, responseOptions);
            }
            else
            {
                var checkboxValues = StorageUtils.PreprocessCheckboxValues(correct);

                for (var i = 0; i < responseOptions.Count; ++i)
                {
                    await AddResponseOption(questionId, responseOptions[i], checkboxValues[i]);
                }
            }
        }

        private async Task AddResponseOptions(
            int questionTypeId, int questionId, List<string> responseOptions, bool[] checkboxValues)
        {
            if (questionTypeId == 3)
            {
                await AddResponseOption(questionId, responseOptions[0]);
            }
            else
            {
                for (var i = 0; i < responseOptions.Count; ++i)
                {
                    await AddResponseOption(questionId, responseOptions[i], checkboxValues[i]);
                }
            }
        }

        private async Task CreateNewTag(string tagName, int questionId)
        {
            var tagId = await _context.TagsInfo.MaxAsync(t => t.TagId) + 1;
            var tag = StorageUtils.CreateTag(tagId, tagName);

            await SaveToDatabase(tag);
            await SaveToDatabase(new TagsQuestions {TagId = tagId, QuestId = questionId});
        }

        private async Task AddTagsToDatabase(StringValues tags, int questionId, QuestionsInfo question = null,
            bool isCreating = true)
        {
            if (question != null && question.IsTemplate)
            {
                await SaveToDatabase(new TagsQuestions {TagId = 1, QuestId = questionId});
            }

            // Adding tags info to database.
            foreach (var tagInfo in tags)
            {
                if (StorageUtils.IsValidTagId(tagInfo))
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
                        await SaveToDatabase(new TagsQuestions {TagId = tagId, QuestId = questionId});
                    }
                }
                else
                {
                    await CreateNewTag(tagInfo, questionId);
                }
            }
        }

        private async Task EditQuestion(int questionId, string questionName, string questionText, string typeName)
        {
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(q => q.QuestId == questionId);
            StorageUtils.EditQuestion(question, questionName, questionText, typeName);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteUnusedTags(int questionId, StringValues tags)
        {
            // Getting tags-questions relation for question before editing
            var tagsQuestions = await _context.TagsQuestions
                .Where(tq => tq.QuestId == questionId).ToListAsync();

            // Getting ids of tags related to question
            var questionTagsIds = new HashSet<int>();
            foreach (var relation in tagsQuestions)
            {
                questionTagsIds.Add(relation.TagId);
            }

            // Getting ids of tags after editing
            var editedTagsIds = new HashSet<int>();
            foreach (var tagInfo in tags.Where(StorageUtils.IsValidTagId))
            {
                var exists = int.TryParse(tagInfo.TrimStart('ŧ'), out var tagId);
                exists &= await _context.TagsInfo.AnyAsync(t => t.TagId == tagId);
                if (exists)
                {
                    editedTagsIds.Add(tagId);
                }
            }

            // Getting no longer used tags ids
            questionTagsIds.ExceptWith(editedTagsIds);

            foreach (var currentId in questionTagsIds)
            {
                var relation = await _context.TagsQuestions
                    .FirstAsync(tq => tq.TagId == currentId && tq.QuestId == questionId);
                _context.TagsQuestions.Remove(relation);

                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> DeleteAllExistingResponseOptions(int id, StringValues responseOptions)
        {
            var responseOptionsCount = responseOptions.Count;
            var oldResponseOptions = await _context.QuestionAnswerVariants
                .Where(qav => qav.QuestId == id).ToListAsync();
            var changed = oldResponseOptions.Count != responseOptionsCount;

            if (!changed)
            {
                for (var i = 0; i < responseOptionsCount; ++i)
                {
                    if (!oldResponseOptions[i].Answer.Equals(responseOptions[i].Trim()))
                    {
                        changed = true;
                        break;
                    }
                }
            }

            _context.QuestionAnswerVariants.RemoveRange(oldResponseOptions);

            return changed;
        }
        

        private const string QuestionText = "Question.QuestionText";
        private const string QuestionName = "Question.QuestionName";
        private const string TypeInfo = "Question.Type.Name";
        private const string AnswerText = "AnswerOption.Answer";
    }
}