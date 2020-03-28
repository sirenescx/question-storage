using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;

namespace QuestionStorage.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly HSE_QuestContext _context;

        public QuestionsController(HSE_QuestContext context)
        {
            _context = context;
        }

        // GET: Questions
        public ActionResult Index()
        {
            return RedirectToAction("List", "Display");
        }

        // GET: Questions/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var question = await _context.QuestionsInfo.FirstOrDefaultAsync(
                m => m.QuestId == id);

            if (question == null)
            {
                return NotFound();
            }

            await _context.QuestionAnswerVariants.Where(
                a => a.QuestId == question.QuestId).ToListAsync();
            await _context.TypesInfo.FirstOrDefaultAsync(
                t => t.TypeId == question.TypeId);
            var tagsQuestions = await _context.TagsQuestions.Where(
                qt => qt.QuestId == question.QuestId).ToListAsync();
            foreach (var tagQuestion in tagsQuestions)
            {
                await _context.TagsInfo.Where(
                    t => t.TagId == tagQuestion.TagId).ToListAsync();
            }

            return View(question);
        }

        private static bool[] PreprocessCheckboxValues(string checkboxValues)
        {
            var values = checkboxValues.Split(',');
            var compressedValues = new List<bool>();
            for (var i = 0; i < values.Length; ++i)
            {
                if (i + 1 < values.Length && values[i + 1] == "on")
                {
                    compressedValues.Add(true);
                    ++i;
                }
                else
                {
                    compressedValues.Add(false);
                }
            }

            return compressedValues.ToArray();
        }

        // GET: Questions/Create
        public async Task<ActionResult> Create()
        {
            ViewData["QuestionTypes"] = await _context.TypesInfo.ToListAsync();
            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var question = new QuestionsInfo
                {
                    QuestionText = collection["QuestionText"].ToString(), 
                    QuestId = await _context.QuestionsInfo.MaxAsync(q => q.QuestId) + 1,
                    TypeId = collection["Type.Name"] == "sc" ? 1 : collection["Type.Name"] == "mc" ? 2 : 3,
                    QuestionName = "New Question",
                    Flags = 0
                };
                
                await _context.AddAsync(question);
                await _context.SaveChangesAsync();

                var checkboxValues = PreprocessCheckboxValues(collection["Correct"]);
                
                for (var i = 0; i < collection["AnswerText"].Count; ++i)
                {
                    var questionAnswerVariant = new QuestionAnswerVariants
                    {
                        VariantId = await _context.QuestionAnswerVariants.MaxAsync(qav => qav.VariantId) + 1,
                        QuestId = question.QuestId,
                        Answer = collection["AnswerText"][i],
                        IsCorrect = checkboxValues[i],
                        SortCode = 0
                    };
                
                    await _context.AddAsync(questionAnswerVariant);
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction("List", "Display");
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Questions/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var question = await _context.QuestionsInfo
                .FirstOrDefaultAsync(m => m.QuestId == id);
            if (question == null)
            {
                return NotFound();
            }

            await _context.QuestionAnswerVariants.Where(
                a => a.QuestId == question.QuestId).ToListAsync();
            var tagsQuestions = await _context.TagsQuestions.Where(
                qt => qt.QuestId == question.QuestId).ToListAsync();
            var currentTags = new List<TagsInfo>();
            foreach (var tagQuestion in tagsQuestions)
            {
                var tagInfo = await _context.TagsInfo.FirstOrDefaultAsync(
                    t => t.TagId == tagQuestion.TagId);
                currentTags.Add(tagInfo);
            }

            var tags = await _context.TagsInfo.ToListAsync();

            ViewData["CurrentTags"] = currentTags;
            ViewData["AllTags"] = tags;

            return View(question);
        }

        //// POST: Questions/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        public ActionResult Display(int id)
        {
            return RedirectToAction("ListByTag", "Display", new {id});
        }
    }
}