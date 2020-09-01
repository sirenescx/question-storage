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
        private readonly HSE_QuestContext _context;
        private readonly IWebHostEnvironment _environment;

        public QuizzesController(HSE_QuestContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                Validator.ValidateTestCreation(collection, ModelState);

                if (!ModelState.IsValid)
                {
                    return View();
                }

                var quiz = await QuizExtensions.CreateQuiz(
                    _context, collection["Name"], collection["Date"]);
                
                await StorageUtils.SaveToDatabase(_context, quiz);

                var identifiers = collection["QuestionId"]
                    .Where(questionId => questionId != string.Empty)
                    .Select(int.Parse)
                    .ToHashSet();

                foreach (var id in identifiers)
                {
                    if (await QuestionExtensions.QuestionExists(_context, id))
                    {
                        await StorageUtils.SaveToDatabase(_context, new QuizzesInfoQuestionsInfo
                        {
                            QuestId = id, QuizId = quiz.QuizId
                        });
                    }
                }

                return View();
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var quiz = await DataStorage.GetByPredicateAsync(_context.QuizzesInfo, 
                quizzesInfo => quizzesInfo.QuizId == id);

            if (quiz == null)
            {
                return ErrorPage(404);
            }
            
            var questionIdentifiers = await GetQuestionIdentifiers(id);

            var questions = new HashSet<QuestionsInfo>();
            
            foreach (var questionId in questionIdentifiers)
            {
                var question =
                    await DataStorage.GetByPredicateAsync(_context.QuestionsInfo, 
                        questionsInfo => questionsInfo.QuestId == questionId);

                questions.Add(question);

                await DataStorage.GetListByPredicateAsync(_context.QuestionAnswerVariants,
                    questionAnswerVariants => questionAnswerVariants.QuestId == questionId);
                
                await DataStorage.GetByPredicateAsync(_context.TypesInfo, 
                    type => type.TypeId == question.TypeId);
            }

            ViewData["Questions"] = questions;

            return View(quiz);
        }

        //TODO: Move ErrorPage for common access of Quizzes and Questions controller
        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;

            return View("Error");
        }

        public async Task<IActionResult> ExportToXml(int id)
        {
            var questionIdentifiers = await GetQuestionIdentifiers(id);

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

        private async Task<HashSet<int>> GetQuestionIdentifiers(int id) =>
            await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                _context.QuizzesInfoQuestionsInfo, 
                quizzesInfoQuestionsInfo => quizzesInfoQuestionsInfo.QuizId == id,
                quizzesInfoQuestionsInfo => quizzesInfoQuestionsInfo.QuestId);
    }
}