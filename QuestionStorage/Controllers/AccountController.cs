using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using QuestionStorage.Models;
using QuestionStorage.Models.Users;
using QuestionStorage.Models.ViewModels;
using QuestionStorage.Utils;

// ReSharper disable VariableHidesOuterVariable
//TODO: Get rid of static variable

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly StorageContext context;
        private readonly MessageSender messageSender;

        public AccountController(StorageContext context)
        {
            this.context = context;
            messageSender = new MessageSender();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ListCourses", "Display");
            }

            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await context.Users.FirstAsync(user => user.Email.Equals(model.Email));

            if (user != null)
            {
                if (user.Password.Equals(StorageUtils.GetPasswordHash(model.Password, Encoding.ASCII)) ||
                    user.Password.Equals(StorageUtils.GetPasswordHash(model.Password, Encoding.Unicode)))
                {
                    await Authenticate(user);

                    return RedirectToAction("ListCourses", "Display");
                }
            }

            ModelState.AddModelError("Email", "Invalid e-mail or password.");

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(IFormCollection collection)
        {
            var email = collection["Email"];
            var user = await context.Users.FirstAsync(user => user.Email.Equals(email));

            if (user is null)
            {
                return RedirectToAction("ForgotPassword");
            }

            var token = StorageUtils.GenerateToken();
            var restorationToken = new RestorationTokens
            {
                Token = token,
                UserId = user.Id
            };

            messageSender.CreateMessage(email, CreateRestorationLink(token), "Restore password");

            await context.AddAsync(restorationToken);
            await context.SaveChangesAsync();

            return View("RestoreMessage");
        }

        private static string CreateRestorationLink(string token) =>
            $"https://localhost:5000/Account/RestorePassword/?token={token}"; 
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePassword(string token)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var restorationToken = await context.RestorationTokens.FirstOrDefaultAsync(
                restorationToken => restorationToken.Token.Equals(token));

            if (!(restorationToken is null) && !restorationToken.Expired && StorageUtils.DecodeToken(token))
            {
                ViewData["Token"] = token;
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePassword(string token, EditViewModel model)
        {
            var restorationToken = await context.RestorationTokens.FirstOrDefaultAsync(
                restorationToken => restorationToken.Token.Equals(token));

            var user = await context.Users.FirstOrDefaultAsync(user => user.Id == restorationToken.UserId);

            UserExtensions.UpdatePassword(model, user);
            restorationToken.Expired = true;
            await context.SaveChangesAsync();

            return View("Login");
        }

        private async Task Authenticate(User user)
        {
            await DataStorage.GetByPredicateAsync(context.Roles, role => role.Id == user.RoleId);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Manage()
        {
            await FillViewData(ViewData);

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(IFormCollection collection)
        {
            var email = collection["Email"];

            var currentUsers = await DataStorage.GetTypedHashSetBySelectorAsync(context.Users,
                user => user.Email);

            Validator.ValidateUserCreation(collection, currentUsers, ModelState);

            if (!ModelState.IsValid)
            {
                await FillViewData(ViewData);

                return View();
            }

            //TODO: Normal password generation
            var password = "qstorage@#_pass";

            await context.AddAsync(await UserExtensions.CreateUser(context, email, password, collection["Admin"]));
            await context.SaveChangesAsync();

            await FillViewData(ViewData);

            return View();
        }

        [HttpGet]
        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewPasswordViewModel model)
        {
            var user = await DataStorage.GetByPredicateAsync(context.Users,
                user => user.Email.Equals(User.Identity.Name));

            Validator.ValidatePasswordChange(model.OldPassword, user.Password, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            UserExtensions.UpdatePassword(model, user);
            await context.SaveChangesAsync();

            ViewData["Success"] = "Password successfully changed";

            return View(model);
        }

        private static string _successMessage;

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> UserInfo(int id)
        {
            var user = await DataStorage.GetByPredicateAsync(context.Users, user => user.Id == id);

            if (_successMessage == null)
            {
                return View(user);
            }

            ViewData["Message"] = _successMessage;
            _successMessage = null;

            return View(user);
        }

        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await DataStorage.GetByPredicateAsync(context.Users, user => user.Id == id);

            //TODO: Normal password generation
            UserExtensions.SetPassword(user);
            await context.SaveChangesAsync();

            StorageUtils.CopyToClipboard(user.Email, user.Password);

            _successMessage = "Password was successfully reset. User data was copied to a clipboard.";

            return RedirectToAction("UserInfo", new {id});
        }

        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> ChangeRole(int id)
        {
            UserExtensions.ChangeRole(
                await DataStorage.GetByPredicateAsync(context.Users, user => user.Id == id));
            await context.SaveChangesAsync();

            return RedirectToAction("UserInfo", new {id});
        }

        private async Task FillViewData(ViewDataDictionary viewData) =>
            viewData["Users"] = await DataStorage.GetListAsync(context.Users);
    }
}