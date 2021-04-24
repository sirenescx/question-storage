using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;
using QuestionStorage.Models.Courses;
using QuestionStorage.Models.Users;

namespace QuestionStorage.Controllers
{
    public class CoursesController : Controller
    {
        private readonly StorageContext context;

        public CoursesController(StorageContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int courseId)
        {
            if (!await Helpers.Common.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            ViewData["QuestionsCount"] = await Helpers.Common.GetQuestionsCountForCourse(context, courseId);

            return course == null
                ? ErrorPage(404)
                : View(course);
        }

        [Authorize(Roles = "administrator")]
        [HttpGet]
        public async Task<IActionResult> Subscribe(int courseId)
        {
            ViewData["Users"] = new HashSet<User>(await context.Users.ToListAsync());

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Subscribe(int courseId, IFormCollection data)
        {
            try
            {
                ViewData["Users"] = new HashSet<User>(await context.Users.ToListAsync());

                var userId = int.Parse(data[nameof(Models.Users.User.Email)]);

                if (!await CheckSubscription(courseId, userId))
                {
                    await context.AddAsync(new UsersCourses {CourseId = courseId, UserId = userId});
                    await context.SaveChangesAsync();
                }

                return View();
            }
            catch
            {
                return ErrorPage(404);
            }
        }

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Create(IFormCollection data)
        {
            ValidateCourse(data, ModelState);

            if (!ModelState.IsValid)
            {
                return View();
            }
            
            var userId = await Helpers.Common.GetUserId(context, User.Identity.Name);

            var course = await context.AddAsync(new Course {Name = data[nameof(Course.Name)]});
            await context.SaveChangesAsync();

            await context.AddAsync(new UsersCourses {CourseId = course.Entity.Id, UserId = userId});
            await context.SaveChangesAsync();

            return RedirectToAction("ListCourses", "Display");
        }

        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Delete(int courseId)
        {
            var course = await context.Courses
                .Include(c => c.UsersCourses)
                .Include(c => c.Questions)
                .ThenInclude(q => q.AnswerOptions)
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.QuizzesQuestions)
                .Where(c => c.Id == courseId)
                .FirstOrDefaultAsync();

            var questions = await context.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.TagsQuestions)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            var quizzes = await context.Quizzes
                .Include(q => q.QuizzesQuestions)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            if (questions != null)
            {
                context.RemoveRange(questions);
            }
            if (quizzes != null)
            {
                context.RemoveRange(quizzes);
            }
            context.Remove(course);

            await context.SaveChangesAsync();

            return RedirectToAction("ListCourses", "Display");
        }

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Edit(int courseId)
        {
            var course = await context.Courses.Where(c => c.Id == courseId).FirstOrDefaultAsync();

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Edit(int courseId, IFormCollection data)
        {
            ValidateCourse(data, ModelState);

            if (!ModelState.IsValid)
            {
                return View();
            }
            
            var course = await context.Courses.Where(c => c.Id == courseId).FirstOrDefaultAsync();

            course.Name = data[nameof(Course.Name)];
            await context.SaveChangesAsync();

            return RedirectToAction("Edit");
        }

        #region Helper Functions

        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;

            return View("Error");
        }

        private void ValidateCourse(IFormCollection data, ModelStateDictionary modelState)
        {
            Helpers.Common.ValidateField(data["Name"], modelState, ("Name", "Course name is required."));
        }
        
        private async Task<bool> CheckSubscription(int courseId, int userId) =>
            await context.UsersCourses.AnyAsync(usersCourses => usersCourses.UserId == userId &&
                                                                usersCourses.CourseId == courseId);

        #endregion
    }
}