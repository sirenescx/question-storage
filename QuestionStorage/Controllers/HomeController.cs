using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using QuestionStorage.Models.ViewModels;

namespace QuestionStorage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ListQuestions", "Display");
            }
            return View();
        }

        [Authorize]
        public IActionResult Questions() => RedirectToAction("ListQuestions", "Display");

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        
        [Authorize(Roles = "administrator")]
        public IActionResult Manage()
        {
            throw new System.NotImplementedException();
        }

        public IActionResult Contact() => View();

        public IActionResult Tests() => RedirectToAction("ListTests", "Display");
    }
}
