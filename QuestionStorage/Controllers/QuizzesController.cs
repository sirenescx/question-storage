using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Collections;
using QuestionStorage.Models.QuizzesQuestionsModels;
using QuestionStorage.Utils;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly HSE_QuestContext _context;

        public QuizzesController(HSE_QuestContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            var testId = await _context.QuizzesInfo.MaxAsync(q => q.QuizId) + 1;
            var test = new QuizzesInfo
            {
                QuizId = testId,
                Name = collection["Name"],
                Date = DateTime.Parse(collection["Date"])
            };

            await StorageUtils.SaveToDatabase(_context, test);

            var ids = (
                from questionId in collection["QuestionId"]
                where questionId != string.Empty
                select int.Parse(questionId)).ToHashSet();

            foreach (var id in ids)
            {
                if (await StorageUtils.QuestionExists(_context, id))
                {
                    await StorageUtils.SaveToDatabase(_context, new QuizzesInfoQuestionsInfo
                    {
                        QuestId = id, QuizId = test.QuizId
                    });
                }
            }

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.QuizzesInfo
                .FirstOrDefaultAsync(m => m.QuizId == id);

            if (test == null)
            {
                return ErrorPage(404);
            }

            var questionsIds = new HashSet<int>(await _context.QuizzesInfoQuestionsInfo
                .Where(q => q.QuizId == id)
                .Select(x => x.QuestId).ToListAsync());

            var questions = new HashSet<QuestionsInfo>();
            foreach (var questionId in questionsIds)
            {
                var question = await _context.QuestionsInfo
                    .Where(q => q.QuestId == questionId).FirstAsync();
                
                questions.Add(question);
                
                await _context.QuestionAnswerVariants
                    .Where(a => a.QuestId == questionId).ToListAsync();
                await _context.TypesInfo.FirstOrDefaultAsync(t => t.TypeId == question.TypeId);
            }

        
            
            ViewData["Questions"] = questions;
   
            return View(test);
        }
        
        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;

            return View("Error");
        }
    }
}