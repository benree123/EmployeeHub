using System.Security.Claims;
using EmployeeManagementSystem.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeManagementSystem.Controllers
{
    public class AccountController(Accounts accounts, IConfiguration configuration) : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(string? username, string? password, string? returnUrl = null)
        {
            var name = username?.Trim().ToLowerInvariant();
            var role = accounts.Validate(name, password);
            if (role is null)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, "Username or password is incorrect.");
                return View();
            }
            await SignIn(name!, role);
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Employees");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("writes")]
        public async Task<IActionResult> Demo()
        {
            if (!configuration.GetValue<bool>("Demo:Enabled")) return NotFound();
            await SignIn("Demo guest", Roles.DemoGuest);
            return RedirectToAction("Index", "Employees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        private Task SignIn(string name, string role)
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Role, role)], CookieAuthenticationDefaults.AuthenticationScheme);
            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
