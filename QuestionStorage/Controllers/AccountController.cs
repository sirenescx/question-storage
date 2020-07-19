using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models.UserDataModels;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Utils;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class AccountController : Controller 
    {
        private readonly UserDataContext _context;

        public AccountController(UserDataContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => 
                        u.Email.Equals(model.Email) && u.Password.Equals(StorageUtils.GetPasswordHash(model.Password)));

                if (user != null)
                {
                    await Authenticate(user);
                    return RedirectToAction("ListQuestions", "Display");
                }
                
                ModelState.AddModelError("Email", "Invalid e-mail or password.");
            }
            
            return View(model);
        }
        
        private async Task Authenticate(User user)
        {
            await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
            };
    
            var id = new ClaimsIdentity(claims, "ApplicationCookie", 
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
  
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public IActionResult Manage()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "administrator")]
        [ValidateAntiForgeryToken]
        public IActionResult Manage(IFormCollection collection)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.Equals(User.Identity.Name));
                user.Password = StorageUtils.GetPasswordHash(model.Password);
                await _context.SaveChangesAsync();
                
                ViewData["Success"] = "Password successfully changed";
            }
            
            return View(model);
        }
    }
}