using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurkSoft.Services.Interfaces;
using TurkSoft.BankWebUI.ViewModels;
using System.Threading.Tasks;

namespace TurkSoft.BankWebUI.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

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

            var isAuthenticated = await _userService.AuthenticateAsync(vm.Email, vm.Password);
            if (!isAuthenticated)
            {
                vm.Error = "E-posta veya şifre hatalı.";
                return View(vm);
            }

            var user = await _userService.GetUserByEmailAsync(vm.Email);
            if (user == null || !user.IsActive)
            {
                vm.Error = "Kullanıcı bulunamadı veya pasif durumda.";
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserName", user.UserName)
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