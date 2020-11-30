using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionStorage.Models;
using QuestionStorage.Models.Courses;
using QuestionStorage.Utils;

namespace QuestionStorage.Controllers
{
    public class CoursesController : Controller
    {
        private readonly StorageContext context;

        public CoursesController(StorageContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Details(int courseId)
        {
            if (!await StorageUtils.CheckAccess(context, courseId, User.Identity.Name))
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var course = await DataStorage.GetByPredicateAsync(context.CoursesInfo,
                coursesInfo => coursesInfo.CourseId == courseId);

            ViewData["QuestionsCount"] = await StorageUtils.GetQuestionsCountForCourse(context, courseId);

            return course == null
                ? ErrorPage(404)
                : View(course);
        }

        //TODO: Move ErrorPage for common access of Quizzes and Questions controller
        private ActionResult ErrorPage(int errorCode)
        {
            Response.StatusCode = errorCode;
            ViewData["StatusCode"] = errorCode;

            return View("Error");
        }

        [Authorize(Roles = "administrator")]
        [HttpGet]
        public async Task<IActionResult> Subscribe(int courseId)
        {
            ViewData["Users"] = await DataStorage.GetTypedHashSetBySelectorAsync(context.Users, user => user);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Subscribe(int courseId, IFormCollection collection)
        {
            try
            {
                ViewData["Users"] = await DataStorage.GetTypedHashSetBySelectorAsync(context.Users, user => user);

                var userId = int.Parse(collection["Email"]);

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

        private async Task<bool> CheckSubscription(int courseId, int userId) =>
            await DataStorage.CheckByPredicateAsync(context.UsersCourses,
                usersCourses => usersCourses.UserId == userId && usersCourses.CourseId == courseId);

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            var userId = await StorageUtils.GetUserId(context, User.Identity.Name);
            var courseId = await DataStorage.GetIdAsync(context.CoursesInfo,
                coursesInfo => coursesInfo.CourseId);

            await context.AddAsync(new UsersCourses {CourseId = courseId, UserId = userId});
            await context.AddAsync(new CoursesInfo {CourseId = courseId, CourseName = collection["CourseName"]});
            await context.SaveChangesAsync();

            return RedirectToAction("ListCourses", "Display");
        }
    }
}