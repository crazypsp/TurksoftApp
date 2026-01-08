using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurkSoft.BankWebUI.Services;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly IDemoDataService _demo;
        public AccountController(IDemoDataService demo) => _demo = demo;

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Giriş";
            return View(new LoginVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            ViewData["Title"] = "Giriş";
            if (!ModelState.IsValid) return View(vm);

            var user = _demo.ValidateUser(vm.Email, vm.Password);
            if (user is null)
            {
                vm.Error = "E-posta veya şifre hatalı. (Demo: admin@firma.com / 123456)";
                return View(vm);
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = vm.RememberMe }
            );

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Erişim Reddedildi";
            return View();
        }
    }
}
