using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QuestionStorage.Models;
using QuestionStorage.Utils;

// ReSharper disable VariableHidesOuterVariable

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class DisplayController : Controller
    {
        private readonly HSE_QuestContext _context;

        public DisplayController(HSE_QuestContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> ListQuestions()
        {
            await FillListQuestionsViewData(ViewData);

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ListQuestions(IFormCollection collection)
        {
            await FillListQuestionsViewData(ViewData);

            var tagIdentifiers = collection["Tags"]
                .Select(int.Parse)
                .ToHashSet();

            var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions, 
                tagsQuestions => tagIdentifiers.Contains(tagsQuestions.TagId));

            var questionIdentifiers = !tagIdentifiers.Any()
                ? GetQuestionIdsWithoutTags()
                : tagsQuestions.Select(tq => tq.QuestId).ToHashSet();

            var questions = await DataStorage.GetListByPredicateAsync(_context.QuestionsInfo, 
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId));

            var lastVersions =
                questions
                    .GroupBy(question => question.SourceQuestId)
                    .Select(versions => versions.OrderBy(version => version.VersionId)
                        .Last());

            if (tagIdentifiers.Any())
            {
                foreach (var question in lastVersions)
                {
                    await DataStorage.GetListByPredicateAsync(_context.TagsQuestions, 
                        tagsQuestions => tagsQuestions.QuestId == question.QuestId);
                }
            }

            return View(lastVersions);
        }

        public async Task<IActionResult> ListByTag(int id)
        {
            var tagsQuestionsInfo = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                tagsQuestions => tagsQuestions.TagId == id);

            var questionIdentifiers = tagsQuestionsInfo
                .Select(tagsQuestions => tagsQuestions.QuestId).ToList();

            var questions = await DataStorage.GetListByPredicateAsync(_context.QuestionsInfo, 
                questionsInfo => questionIdentifiers.Contains(questionsInfo.QuestId));

            foreach (var question in questions)
            {
                var tagsQuestions = await DataStorage.GetListByPredicateAsync(_context.TagsQuestions,
                    tagsQuestions => tagsQuestions.QuestId == question.QuestId);

                foreach (var tagQuestion in tagsQuestions)
                {
                    await DataStorage.GetListByPredicateAsync(_context.TagsInfo, 
                        tagsInfo => tagsInfo.TagId == tagQuestion.TagId);
                }
            }

            ViewData["TagName"] = tagsQuestionsInfo.First().Tag.Name;

            return View(questions);
        }

        public IActionResult AboutQuestion(int id) =>
            RedirectToAction("Details", "Questions", new {id});

        [HttpGet]
        public async Task<IActionResult> ListTests()
        {
            var tests = await DataStorage.GetListAsync(_context.QuizzesInfo);

            return View(tests);
        }

        public IActionResult AboutTest(int id) =>
            RedirectToAction("Details", "Quizzes", new {id});

        private async Task FillListQuestionsViewData(ViewDataDictionary viewData)
        {
            viewData["Tags"] = await DataStorage.GetHashSetAsync(_context.TagsInfo);
            viewData["MetaTags"] = await DataStorage.GetTypedHashSetByPredicateAndSelectorAsync(
                _context.TagsInfo,
                tagsInfo => tagsInfo.IsMetaTag == true,
                tagsInfo => tagsInfo.TagId);
        }

        private HashSet<int> GetQuestionIdsWithoutTags()
        {
            var questionIdentifiers = DataStorage.GetTypedHashSetBySelector(
                _context.QuestionsInfo, questionsInfo => questionsInfo.QuestId);

            questionIdentifiers.ExceptWith(DataStorage.GetTypedHashSetBySelector(
                _context.TagsQuestions, tagsQuestions => tagsQuestions.QuestId));

            return questionIdentifiers;
        }
    }
}