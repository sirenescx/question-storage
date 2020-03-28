using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuestionStorage.Controllers
{
    public class DisplayController : Controller
    {
        private readonly HSE_QuestContext _context;

        public DisplayController(HSE_QuestContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> List()
        {
            var questions = await _context.QuestionsInfo.ToListAsync();
            foreach (var question in questions)
            {
                var tagsQuestions =
                    await _context.TagsQuestions.Where(qt => qt.QuestId == question.QuestId).ToListAsync();

                foreach (var tagQuestion in tagsQuestions)
                {
                    await _context.TagsInfo.Where(t => t.TagId == tagQuestion.TagId).ToListAsync();
                }
            }

            return View(questions);
        }

        public async Task<ActionResult> ListByTag(int id)
        {
            var tagsQuestionsInfo = await _context.TagsQuestions.Where(tq => tq.TagId == id).ToListAsync();
            var questionsId = tagsQuestionsInfo.Select(tq => tq.QuestId).ToList();
            var questions = await _context.QuestionsInfo.Where(q => questionsId.Contains(q.QuestId)).ToListAsync();
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