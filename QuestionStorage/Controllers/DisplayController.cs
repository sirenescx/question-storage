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
using QuestionStorage.Utils;

// ReSharper disable VariableHidesOuterVariable

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
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            if (string.IsNullOrEmpty(questions))
            {
                return View();
            }

            var questionIdentifiers = Parser.ParseIdentifiers(questions);

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => questionIdentifiers.Contains(tagsQuestions.QuestId));

            var questionsList = await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId) &&
                                 questionsInfo.CourseId == courseId,
                questionsInfo => questionsInfo);

            var tagIdentifiers = tagsQuestions.Select(tagsQuestions => tagsQuestions.TagId).ToHashSet();

            if (tagIdentifiers.Any())
            {
                foreach (var question in questionsList)
                {
                    await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                        tagsQuestions => tagsQuestions.QuestId == question.QuestId);
                }
            }

            return View(new ListQuestionsViewModel
            {
                QuestionsInfo = questionsList,
                TagsInfo = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(context.TagsInfo,
                    tagsInfo => tagIdentifiers.Contains(tagsInfo.TagId),
                    tagsInfo => tagsInfo)
            });
        }

        [HttpGet]
        public async Task<ActionResult> ListQuestions(int courseId, string tags)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            List<QuestionsInfo> lastVersions;

            if (tags is null)
            {
                lastVersions = (await context.QuestionsInfo
                        .Include(questionsInfo => questionsInfo.TagsQuestions)
                        .ThenInclude(tagsQuestions => tagsQuestions.Tag)
                        .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync())
                        .GroupBy(question => question.SourceQuestId)
                        .Select(versions => versions.OrderBy(version => version.VersionId)
                            .LastOrDefault()).ToList();

                return View(new ListQuestionsViewModel
                {
                    QuestionsInfo = lastVersions,
                    TagsInfo = await GetAllTags(courseId)
                });
            }

            var tagIdentifiers = Parser.ParseIdentifiers(tags);

            var questions = tagIdentifiers.Contains(0)
                ? await context.QuestionsInfo.Where(questionsInfo => questionsInfo.TagsQuestions.Count == 0)
                    .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync()
                : await context.QuestionsInfo
                    .Include(questionsInfo => questionsInfo.TagsQuestions)
                    .ThenInclude(tagsQuestions => tagsQuestions.Tag)
                    .Where(tagsQuestions => tagsQuestions.TagsQuestions.Any(tag => tagIdentifiers.Contains(tag.TagId)))
                    .Where(questionsInfo => questionsInfo.CourseId == courseId).ToListAsync();

            lastVersions =
                questions
                    .GroupBy(question => question.SourceQuestId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last()).ToList();

            return View(new ListQuestionsViewModel
            {
                QuestionsInfo = lastVersions,
                TagsInfo = await GetAllTags(courseId)
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

        public async Task<IActionResult> ListQuestionsByTag(int courseId, int tagId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }
            
            var tagsQuestions = await context.TagsQuestions
                .Include(tagsQuestions => tagsQuestions.Quest)
                .Include(tagsQuestions => tagsQuestions.Tag)
                .Where(tagsQuestions => tagsQuestions.TagId == tagId).ToListAsync();
            
            var questions = tagsQuestions
                .Select(tagsQuestions => tagsQuestions.Quest)
                .GroupBy(question => question.SourceQuestId)
                .Select(versions => versions
                    .OrderBy(version => version.VersionId)
                    .Last())
                .ToHashSet();

            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> ListTests(int courseId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            var tests = await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuizzesInfo,
                quizzesInfo => quizzesInfo.CourseId == courseId,
                quizzesInfo => quizzesInfo);

            return View(tests);
        }

        [HttpGet]
        public async Task<IActionResult> ListCourses()
        {
            var userId = await StorageUtils.GetUserId(context, User.Identity.Name);

            var userCoursesIdentifiers = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                context.UsersCourses,
                usersCourses => usersCourses.UserId == userId,
                usersCourses => usersCourses.CourseId);

            if (userCoursesIdentifiers.Count == 1)
            {
                return RedirectToAction("Details", "Courses", new {courseId = userCoursesIdentifiers.Max()});
            }

            var courses = await DataStorage.GetListByPredicateAsync(context.CoursesInfo,
                coursesInfo => userCoursesIdentifiers.Contains(coursesInfo.CourseId));

            var questionsCount = new List<int>();

            foreach (var course in courses)
            {
                questionsCount.Add(await StorageUtils.GetQuestionsCountForCourse(context, course.CourseId));
            }

            ViewData["QuestionsCount"] = questionsCount;

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> AboutQuestion(int courseId, int questionId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Questions", new {courseId, questionId});
        }

        [HttpGet]
        public async Task<IActionResult> AboutTest(int id)
        {
            if (!await CheckAccessByQuiz(id))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Quizzes", new {id});
        }

        private async Task<HashSet<TagsInfo>> GetAllTags(int id) =>
            await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(context.TagsInfo,
                tagsInfo => tagsInfo.CourseId == id,
                tagsInfo => tagsInfo);

        private HashSet<int> GetQuestionIdsWithoutTags()
        {
            var questionIdentifiers = DataStorage.GetTypedHashSetBySelector(
                context.QuestionsInfo, questionsInfo => questionsInfo.QuestId);

            questionIdentifiers.ExceptWith(DataStorage.GetTypedHashSetBySelector(
                context.TagsQuestions, tagsQuestions => tagsQuestions.QuestId));

            return questionIdentifiers;
        }

        private async Task<bool> CheckAccessByQuiz(int questionId)
        {
            var userId = await StorageUtils.GetUserId(context, User.Identity.Name);

            var quiz = await DataStorage.GetByPredicateAsync(context.QuizzesInfo,
                quizzesInfo => quizzesInfo.QuizId == questionId);

            var userCoursesIdentifiers = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                context.UsersCourses,
                usersCourses => usersCourses.UserId == userId,
                usersCourses => usersCourses.CourseId);

            return userCoursesIdentifiers.Contains(quiz.CourseId);
        }
    }
}