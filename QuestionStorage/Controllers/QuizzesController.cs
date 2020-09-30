using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionStorage.Models;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Utils;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly StorageContext context;
        private readonly IWebHostEnvironment environment;

        public QuizzesController(StorageContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        [HttpGet]
        public IActionResult Create(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, IFormCollection collection)
        {
            try
            {
                var quiz = await CreateTest(id, collection);

                return RedirectToAction("Details", new {id = quiz.QuizId});
            }
            catch
            {
                return ErrorPage(404, View("Error"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz =  await DataStorage.GetByPredicateAsync(context.QuizzesInfo, 
                quizzesInfo => quizzesInfo.QuizId == id);

            if (quiz == null)
            {
                return ErrorPage(404, View("Error"));
            }
            
            var date = quiz.Date;
            
            ViewData["QuestionIdentifiers"] = await GetQuestionIdentifiers(id);
            ViewData["QuizDate"] = $"{date.Year}-{date.Month:d2}-{date.Day:d2}";

            return View(quiz);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            var quiz = await CreateTest(id, collection);

            return RedirectToAction("Details", new {id = quiz.QuizId});
        }

        public async Task<IActionResult> Details(int id)
        {
            var quiz = await DataStorage.GetByPredicateAsync(context.QuizzesInfo, 
                quizzesInfo => quizzesInfo.QuizId == id);

            if (quiz == null)
            {
                return ErrorPage(404, View("Error"));
            }
            
            var questionIdentifiers = await GetQuestionIdentifiers(id);

            var questions = new HashSet<QuestionsInfo>();
            
            foreach (var questionId in questionIdentifiers)
            {
                var question =
                    await DataStorage.GetByPredicateAsync(context.QuestionsInfo, 
                        questionsInfo => questionsInfo.QuestId == questionId);

                questions.Add(question);

                await DataStorage.GetListByPredicateAsync(context.QuestionAnswerVariants,
                    questionAnswerVariants => questionAnswerVariants.QuestId == questionId);
                
                await DataStorage.GetByPredicateAsync(context.TypesInfo, 
                    type => type.TypeId == question.TypeId);
            }

            ViewData["Questions"] = questions;

            return View(quiz);
        }

        //TODO: Move ErrorPage for common access of Quizzes and Questions controller
        private ActionResult ErrorPage(int errorCode, ViewResult view)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;
            
            return view;
        }

        public async Task<IActionResult> ExportToXml(int id)
        {
            var questionIdentifiers = await GetQuestionIdentifiers(id);

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

        private async Task<HashSet<int>> GetQuestionIdentifiers(int id) =>
            await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                context.QuizzesInfoQuestionsInfo, 
                quizzesInfoQuestionsInfo => quizzesInfoQuestionsInfo.QuizId == id,
                quizzesInfoQuestionsInfo => quizzesInfoQuestionsInfo.QuestId);

        private async Task<QuizzesInfo> CreateTest(int courseId, IFormCollection collection)
        {
            void RedirectToPage() => View();
            
            Validator.ValidateTestCreation(collection, ModelState);

            if (!ModelState.IsValid)
            {
                RedirectToPage();
                
                return null;
            }

            var quiz = await QuizExtensions.CreateQuiz(
                context, collection["Name"], collection["Date"], courseId);
                
            await StorageUtils.SaveToDatabase(context, quiz);

            var identifiers = collection["QuestionId"]
                .Where(questionId => questionId != string.Empty)
                .Select(int.Parse)
                .ToHashSet();

            foreach (var id in identifiers)
            {
                if (await QuestionExtensions.QuestionExists(context, id))
                {
                    await StorageUtils.SaveToDatabase(context, new QuizzesInfoQuestionsInfo
                    {
                        QuestId = id, QuizId = quiz.QuizId
                    });
                }
            }

            return quiz;
        }
    }
}