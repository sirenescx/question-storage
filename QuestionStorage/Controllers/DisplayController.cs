using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QuestionStorage.Models;
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
        public async Task<ActionResult> ListQuestions(int courseId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            return View(new ListQuestionsViewModel {TagsInfo = await FillListQuestionsViewData(ViewData, courseId)});
        }

        [HttpPost]
        public async Task<IActionResult> ListQuestions(int courseId, IFormCollection collection)
        {
            var tagIdentifiers = collection["Tags"]
                .Select(int.Parse)
                .ToHashSet();

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagIdentifiers.Contains(tagsQuestions.TagId));

            var questionIdentifiers = !tagIdentifiers.Any()
                ? GetQuestionIdsWithoutTags()
                : tagsQuestions.Select(tq => tq.QuestId).ToHashSet();

            var questions = await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId),
                questionsInfo => questionsInfo);

            var lastVersions =
                questions
                    .GroupBy(question => question.SourceQuestId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last()).ToList();

            if (tagIdentifiers.Any())
            {
                foreach (var question in lastVersions)
                {
                    await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                        tagsQuestions => tagsQuestions.QuestId == question.QuestId);
                }
            }

            return View(new ListQuestionsViewModel
            {
                QuestionsInfo = lastVersions, 
                TagsInfo = await FillListQuestionsViewData(ViewData, courseId)
            });
        }
        
        public async Task<IActionResult> ListQuestionsByTag(int courseId, int tagId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            var tagsQuestionsInfo = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagsQuestions.TagId == tagId);

            var questionIdentifiers = tagsQuestionsInfo
                .Select(tagsQuestions => tagsQuestions.QuestId).ToList();

            var questions = await DataStorage.GetListByPredicateAsync(context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId));

            foreach (var question in questions)
            {
                var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                    tagsQuestions => tagsQuestions.QuestId == question.QuestId);

                foreach (var tagQuestion in tagsQuestions)
                {
                    await DataStorage.GetListByPredicateAsync(context.TagsInfo,
                        tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
                }
            }

            ViewData["TagName"] = tagsQuestionsInfo.First().Tag.Name;

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

            return View(courses);
        }

        public async Task<IActionResult> AboutQuestion(int courseId, int questionId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Questions", new {courseId, questionId});
        }


        public async Task<IActionResult> AboutTest(int id)
        {
            if (!await CheckAccessByQuiz(id))
            {
                return RedirectToAction("ListCourses");
            }

            return RedirectToAction("Details", "Quizzes", new {id});
        }

        private async Task<HashSet<TagsInfo>> FillListQuestionsViewData(ViewDataDictionary viewData, int id) =>
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