using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;
using QuestionStorage.Models.Questions;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.ViewModels;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class DisplayController : Controller
    {
        private readonly StorageContext context;

        public DisplayController(StorageContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List(int courseId, string questions)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            if (string.IsNullOrEmpty(questions))
            {
                return View();
            }

            var questionIdentifiers = ParseIdentifiers(questions);

            var tagsQuestions = await context.TagsQuestions
                .Where(tq => questionIdentifiers.Contains(tq.QuestionId))
                .ToListAsync();

            var questionsList = await context.Questions
                .Include(q => q.TagsQuestions)
                .Where(q => questionIdentifiers.Contains(q.Id) && q.CourseId == courseId)
                .ToListAsync();

            var tagIdentifiers = tagsQuestions.Select(tq => tq.TagId).ToHashSet();

            if (tagIdentifiers.Any())
            {
                foreach (var question in questionsList)
                {
                    await context.TagsQuestions
                        .Where(tq => tq.QuestionId == question.Id)
                        .ToListAsync();
                }
            }

            return View(new ListingViewModel
            {
                Questions = questionsList,
                Tags = new HashSet<Tag>(await context.Tags
                    .Where(t => tagIdentifiers.Contains(t.Id))
                    .ToListAsync())
            });
        }

        [HttpGet]
        public async Task<ActionResult> ListQuestions(int courseId, string tags)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            List<Question> lastVersions;

            if (tags is null)
            {
                lastVersions = (await context.Questions
                        .Include(questionsInfo => questionsInfo.TagsQuestions)
                        .ThenInclude(tagsQuestions => tagsQuestions.Tag)
                        .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync())
                    .GroupBy(question => question.SourceId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .LastOrDefault()).ToList();

                return View(new ListingViewModel
                {
                    Questions = lastVersions,
                    Tags = await GetAllTags(courseId)
                });
            }

            var tagIdentifiers = ParseIdentifiers(tags);

            var questions = tagIdentifiers.Contains(0)
                ? await context.Questions.Where(questionsInfo => questionsInfo.TagsQuestions.Count == 0)
                    .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync()
                : await context.Questions
                    .Include(questionsInfo => questionsInfo.TagsQuestions)
                    .ThenInclude(tagsQuestions => tagsQuestions.Tag)
                    .Where(tagsQuestions => tagsQuestions.TagsQuestions.Any(tag => tagIdentifiers.Contains(tag.TagId)))
                    .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync();

            lastVersions =
                questions
                    .GroupBy(question => question.SourceId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last()).ToList();

            return View(new ListingViewModel
            {
                Questions = lastVersions,
                Tags = await GetAllTags(courseId)
            });
        }

        [HttpPost]
        public IActionResult ListQuestions(int courseId, IFormCollection collection)
        {
            var tagIdentifiers = collection["Tags"]
                .Select(int.Parse)
                .ToHashSet();

            return RedirectToAction("ListQuestions",
                new {courseId, tags = tagIdentifiers.Any() ? string.Join('&', tagIdentifiers) : 0.ToString()});
        }

        [HttpGet]
        public async Task<IActionResult> ListQuestionsByTag(int courseId, int tagId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            var tagsQuestions = await context.TagsQuestions
                .Include(tq => tq.Question)
                .Include(tq => tq.Tag)
                .Where(tq => tq.TagId == tagId).ToListAsync();

            var questions = tagsQuestions
                .Select(tq => tq.Question)
                .GroupBy(q => q.SourceId)
                .Select(versions => versions
                    .OrderBy(v => v.VersionId)
                    .Last())
                .ToHashSet();

            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> ListTests(int courseId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            var tests = await context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return View(tests);
        }

        [HttpGet]
        public async Task<IActionResult> ListCourses()
        {
            var userId = await Helpers.Common.GetUserId(context, User.Identity.Name);

            var userCoursesIdentifiers = new HashSet<int>(await context.UsersCourses
                .Where(u => u.UserId == userId)
                .Select(u => u.CourseId)
                .ToListAsync());

            if (userCoursesIdentifiers.Count == 1)
            {
                return RedirectToAction("Details", "Courses", new {courseId = userCoursesIdentifiers.Max()});
            }

            var courses = await context.Courses
                .Where(c => userCoursesIdentifiers.Contains(c.Id))
                .ToListAsync();

            var questionsCount = new List<int>();

            foreach (var course in courses)
            {
                questionsCount.Add(await Helpers.Common.GetQuestionsCountForCourse(context, course.Id));
            }

            ViewData["QuestionsCount"] = questionsCount;

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> AboutQuestion(int courseId, int questionId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Questions", new {courseId, questionId});
        }

        [HttpGet]
        public async Task<IActionResult> AboutTest(int courseId, int quizId)
        {
            if (!await CheckAccessByQuiz(quizId))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Quizzes", new {courseId, quizId});
        }

        #region Helper Functions

        private async Task<HashSet<Tag>> GetAllTags(int id) =>
            new HashSet<Tag>(await context.Tags
                .Where(t => t.CourseId == id)
                .ToListAsync());

        private static HashSet<int> ParseIdentifiers(string query) =>
            new HashSet<int>(Array.ConvertAll(query.Split('&'), int.Parse));

        private async Task<bool> CheckAccessByQuiz(int quizId)
        {
            var userId = await Helpers.Common.GetUserId(context, User.Identity.Name);
            var quiz = await context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            var userCoursesIdentifiers = new HashSet<int>(await context.UsersCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync());

            return userCoursesIdentifiers.Contains(quiz.CourseId);
        }

        #endregion
    }
}