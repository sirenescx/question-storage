using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Helpers.Xml;

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
        public IActionResult Create(int courseId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, IFormCollection data)
        {
            try
            {
                var quiz = await CreateTest(courseId, data);

                if (quiz is null)
                {
                    return View();
                }

                return RedirectToAction("Details", new {courseId, quizId = quiz.Id});
            }
            catch
            {
                return ErrorPage(404, View("Error"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int courseId, int quizId)
        {
            var quiz = await context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId && q.CourseId == courseId);

            if (quiz == null)
            {
                return ErrorPage(404, View("Error"));
            }

            var date = quiz.Date;

            ViewData["QuestionIdentifiers"] = await GetQuestionIdentifiers(quizId);
            ViewData["QuizDate"] = $"{date.Year}-{date.Month:d2}-{date.Day:d2}";

            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int courseId, int quizId, IFormCollection data)
        {
            var quiz = await CreateTest(courseId, data);

            return RedirectToAction("Details", new {courseId = quiz.CourseId, quizId = quiz.Id});
        }

        public async Task<IActionResult> Details(int courseId, int quizId)
        {
            var quiz = await context.Quizzes
                .Include(q => q.QuizzesQuestions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return ErrorPage(404, View("Error"));
            }

            var quizzesQuestions = quiz.QuizzesQuestions;
            var questions = new HashSet<Question>();

            foreach (var quizQuestion in quizzesQuestions)
            {
                var question = await context.Questions
                    .Include(q => q.AnswerOptions)
                    .Include(q => q.Type)
                    .FirstOrDefaultAsync(q => q.Id == quizQuestion.QuestionId);

                questions.Add(question);
            }

            ViewData["Questions"] = questions;

            return View(quiz);
        }

        #region Helper Functions

        private static async Task<bool> QuestionExists(StorageContext context, int id) =>
            await context.Questions.AnyAsync(q => q.Id == id);

        //TODO:
        public async Task<IActionResult> ExportToXml(int id)
        {
            var questionIdentifiers = await GetQuestionIdentifiers(id);
            var questions = await context.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIdentifiers.Contains(q.Id))
                .ToListAsync();
            var responseOptions = questions.Select(question => question.AnswerOptions.ToList()).ToList();
            var document = XmlGenerator.ExportQuestionsToXml(questions, responseOptions, environment);

            return File(Encoding.UTF8.GetBytes(document.OuterXml),
                "application/xml", "questions.xml");
        }

        private ActionResult ErrorPage(int errorCode, ViewResult view)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;

            return view;
        }

        private async Task<HashSet<int>> GetQuestionIdentifiers(int id) =>
            new HashSet<int>(await context.QuizzesQuestions
                .Where(qq => qq.QuizId == id)
                .Select(qq => qq.QuestionId)
                .ToListAsync());

        private static Quiz CreateQuiz(string name, string date, int courseId) =>
            new Quiz
            {
                Name = name,
                Date = DateTime.Parse(date),
                CourseId = courseId
            };

        private static void ValidateTestCreation(IFormCollection collection, ModelStateDictionary modelState)
        {
            if (collection["QuestionId"].Count(string.IsNullOrWhiteSpace) >= 3)
            {
                modelState.AddModelError("QuestionId", "Test should contain at least 3 questions.");
            }

            Helpers.Common.ValidateField(collection["Date"], modelState, ("Date", "Date is required."));
            Helpers.Common.ValidateField(collection["Name"], modelState, ("Name", "Name is required."));
        }

        private async Task<Quiz> CreateTest(int courseId, IFormCollection data)
        {
            // IActionResult RedirectToPage() => View();

            ValidateTestCreation(data, ModelState);

            if (!ModelState.IsValid)
            {
                // RedirectToPage();
                //
                return null;
            }

            var quiz = CreateQuiz(data[nameof(Quiz.Name)], data[nameof(Quiz.Date)], courseId);
            await Helpers.Common.SaveToDatabase(context, quiz);

            var identifiers = data["QuestionId"]
                .Where(questionId => questionId != string.Empty)
                .Select(int.Parse)
                .ToHashSet();

            foreach (var id in identifiers)
            {
                if (await QuestionExists(context, id))
                {
                    await Helpers.Common.SaveToDatabase(context, new QuizzesQuestions
                    {
                        QuestionId = id, QuizId = quiz.Id
                    });
                }
            }

            return quiz;
        }

        #endregion
    }
}