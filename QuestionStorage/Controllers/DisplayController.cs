using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models.QuizzesQuestionsModels;

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
            ViewData["Tags"] = new HashSet<TagsInfo>(await _context.TagsInfo.ToListAsync());

            return View();
        }

        private HashSet<int> FindQuestionsWithoutTagsIds()
        {
            var questionIds = _context.QuestionsInfo.Select(q => q.QuestId).ToHashSet();
            var questionsWithTagsIds = _context.TagsQuestions.Select(tq => tq.QuestId).ToHashSet();
            questionIds.ExceptWith(questionsWithTagsIds);

            return questionIds;
        }
        
        public async Task<IActionResult> ListQuestions(IFormCollection collection)
        {
            ViewData["Tags"] = new HashSet<TagsInfo>(await _context.TagsInfo.ToListAsync());

            var tagsIds = collection["Tags"].Select(int.Parse).ToHashSet();
            var tagsQuestions = await _context.TagsQuestions
                .Where(qt => tagsIds.Contains(qt.TagId)).ToListAsync();
            var questionsIds = tagsQuestions.Select(tq => tq.QuestId).ToHashSet();
            
            List<QuestionsInfo> questions;
            
            if (collection["Tags"].Count == 0)
            {
                var ids = FindQuestionsWithoutTagsIds();
                questions = await _context.QuestionsInfo.Where(q => ids.Contains(q.QuestId)).ToListAsync();
            }
            else
            {
                questions = await _context.QuestionsInfo
                    .Where(q => questionsIds.Contains(q.QuestId)).ToListAsync();
                foreach (var question in questions)
                {
                    tagsQuestions = 
                        await _context.TagsQuestions.Where(tq => tq.QuestId == question.QuestId).ToListAsync();
                }
            }
            
            return View(questions);
        }

        public async Task<IActionResult> ListByTag(int id)
        {
            var tagsQuestionsInfo = await _context.TagsQuestions.Where(
                tq => tq.TagId == id).ToListAsync();
            var questionsId = tagsQuestionsInfo.Select(tq => tq.QuestId).ToList();
            var questions = await _context.QuestionsInfo.Where(
                q => questionsId.Contains(q.QuestId)).ToListAsync();
            foreach (var question in questions)
            {
                var tagsQuestions =
                    await _context.TagsQuestions.Where(qt => qt.QuestId == question.QuestId).ToListAsync();

                foreach (var tagQuestion in tagsQuestions)
                {
                    await _context.TagsInfo.Where(t => t.TagId == tagQuestion.TagId).ToListAsync();
                }
            }

            ViewData["TagName"] = tagsQuestionsInfo.First().Tag.Name;

            return View(questions);
        }
        

        public IActionResult About(int id)
        {
            return RedirectToAction("Details", "Questions", new { id });
        }
    }
}