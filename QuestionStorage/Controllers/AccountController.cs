using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using QuestionStorage.Helpers;
using QuestionStorage.Models;
using QuestionStorage.Models.Users;
using QuestionStorage.Models.ViewModels;

namespace QuestionStorage.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly StorageContext context;
        private readonly MessageSender messageSender;
        private readonly IOptionsMonitor<Models.Options.UserOptions> userOptions;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AccountController(StorageContext context,
            IOptionsMonitor<Models.Options.UserOptions> userOptions, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.userOptions = userOptions;
            this.httpContextAccessor = httpContextAccessor;
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

            var user = await GetUser(model.Email);
            if (user != null)
            {
                if (CheckPassword(model, user))
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
            var email = collection[nameof(Models.Users.User.Email)];
            var user = await GetUser(email);
            if (user != null)
            {
                await SendPasswordLink(user, email, "Restore password");
            }

            return View("RestoreMessage");
        }

        private string CreateRestorationLink(string token) =>
            $"https://{httpContextAccessor.HttpContext.Request.Host.Value}/Account/RestorePassword/?token={token}";

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePassword(string token)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ListCourses", "Display");
            }

            var restorationToken = await context.RestorationTokens.FirstOrDefaultAsync(
                rt => rt.Token.Equals(token));

            if (!(restorationToken is null) && !restorationToken.Expired && DecodeToken(token))
            {
                ViewData["Token"] = token;
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePassword(string token, ChangePasswordViewModel model)
        {
            var restorationToken = await context.RestorationTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token.Equals(token));

            UpdatePassword(model, restorationToken.User);
            restorationToken.Expired = true;
            await context.SaveChangesAsync();

            return View("Login");
        }

        private async Task Authenticate(User user)
        {
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

        [HttpGet]
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
        public async Task<IActionResult> Manage(IFormCollection data)
        {
            var email = data[nameof(Models.Users.User.Email)];
            var users = new HashSet<string>(await context.Users.Select(u => u.Email).ToListAsync());

            ValidateUserCreation(data, users, ModelState);

            if (!ModelState.IsValid)
            {
                await FillViewData(ViewData);

                return View();
            }

            var user = await context.AddAsync(
                CreateUser(email, userOptions.CurrentValue.DefaultPassword, data["Admin"]));
            await context.SaveChangesAsync();

            await SendPasswordLink(user.Entity, email, "Set password");

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
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Equals(User.Identity.Name));

            ValidatePasswordChange(model.OldPassword, user.Password, ModelState);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            UpdatePassword(model, user);
            await context.SaveChangesAsync();

            ViewData["Success"] = "Password successfully changed";

            return View(model);
        }

        private static string _successMessage;

        [HttpGet]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> UserInfo(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (_successMessage == null)
            {
                return View(user);
            }

            ViewData["Message"] = _successMessage;
            _successMessage = null;

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> ChangeRole(int id)
        {
            ChangeRole(await context.Users.FirstOrDefaultAsync(u => u.Id == id));
            await context.SaveChangesAsync();

            return RedirectToAction("UserInfo", new {id});
        }

        #region Helper Functions

        private static bool CheckPassword(LoginViewModel model, User user) =>
            user.Password.Equals(GetPasswordHash(model.Password, Encoding.ASCII)) ||
            user.Password.Equals(GetPasswordHash(model.Password, Encoding.Unicode));

        private async Task<User> GetUser(string email) =>
            await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.Equals(email));

        private static string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        private static bool DecodeToken(string token)
        {
            byte[] data = Convert.FromBase64String(token);
            var when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));

            return when >= DateTime.UtcNow.AddHours(-24);
        }

        private static void ChangeRole(User user) => user.RoleId = user.RoleId == 1 ? 2 : 1;

        private static string GetHashStringFromByteArray(byte[] hash)
        {
            var hashString = new StringBuilder();
            foreach (byte @byte in hash)
            {
                hashString.Append(@byte);
            }

            return hashString.ToString();
        }

        private static string GetPasswordHash(string password, Encoding enc = null)
        {
            enc ??= Encoding.ASCII;

            return GetHashStringFromByteArray(new SHA1CryptoServiceProvider().ComputeHash(enc.GetBytes(password)));
        }

        private static void UpdatePassword(ChangePasswordViewModel model, User user) =>
            user.Password = GetPasswordHash(model.NewPassword);

        private static User CreateUser(string email, string password, StringValues roleInfo) =>
            new User
            {
                Email = email,
                Password = GetPasswordHash(password),
                RoleId = Helpers.Common.PreprocessCheckboxValues(roleInfo).First() ? 1 : 2
            };

        private async Task SendPasswordLink(User user, string email, string subject)
        {
            string token = GenerateToken();
            var restorationToken = new RestorationToken {Token = token, UserId = user.Id};

            messageSender.CreateMessage(email, CreateRestorationLink(token), subject);

            await context.AddAsync(restorationToken);
            await context.SaveChangesAsync();
        }

        private static void ValidateUserCreation(IFormCollection collection, HashSet<string> currentUsers,
            ModelStateDictionary modelState)
        {
            var email = collection["Email"][0].ToLower();
            var regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            if (!regex.Match(email).Success)
            {
                modelState.AddModelError("Email", "Invalid email format (should be jsmith@example.com).");
                return;
            }

            if (currentUsers.Contains(email))
            {
                modelState.AddModelError("Email", "User with such email already exists.");
            }
        }

        private static void ValidatePasswordChange(string enteredPassword, string password,
            ModelStateDictionary modelState)
        {
            Helpers.Common.ValidateField(enteredPassword, modelState, ("OldPassword", "Old password is required"));

            if (GetPasswordHash(enteredPassword) != password)
            {
                modelState.AddModelError("OldPassword", "Old password is incorrect");
            }
        }

        private async Task FillViewData(ViewDataDictionary viewData) =>
            viewData["Users"] = await context.Users.ToListAsync();

        #endregion
    }
}