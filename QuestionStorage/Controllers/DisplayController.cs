using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            
            var questionIdentifiers = new HashSet<int>(Array.ConvertAll(questions.Split('&'), int.Parse));
            
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
            
            var lastVersions = new List<QuestionsInfo>();

            if (tags is null)
            {
                lastVersions =
                    (await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuestionsInfo,
                        questionsInfo => questionsInfo.CourseId == courseId,
                        questionsInfo => questionsInfo))
                        .GroupBy(question => question.SourceQuestId)
                        .Select(versions => versions.OrderBy(version => version.VersionId)
                            .LastOrDefault()).ToList();
                
                foreach (var question in lastVersions)
                {
                    await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                        tagsQuestions => tagsQuestions.QuestId == question.QuestId);
                }
                
                
                return View(new ListQuestionsViewModel
                {
                    QuestionsInfo = lastVersions,
                    TagsInfo = await GetAllTags(courseId)
                });
            }

            var tagIdentifiers = new HashSet<int>(Array.ConvertAll(tags.Split('&'), int.Parse));

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(context.TagsQuestions,
                tagsQuestions => tagIdentifiers.Contains(tagsQuestions.TagId));

            var questionIdentifiers = tagIdentifiers.Contains(0)
                ? GetQuestionIdsWithoutTags()
                : tagsQuestions.Select(tq => tq.QuestId).ToHashSet();

            var questions = await DataStorage.GetTypedListByPredicateAndSelectorAsync(context.QuestionsInfo,
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId) &&
                                 questionsInfo.CourseId == courseId,
                questionsInfo => questionsInfo);

            lastVersions =
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
                TagsInfo = await GetAllTags(courseId)
            });
        }

        [HttpPost]
        public async Task<IActionResult> ListQuestions(int courseId, IFormCollection collection)
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