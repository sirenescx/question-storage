using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuestionStorage.Models;
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
    }
}