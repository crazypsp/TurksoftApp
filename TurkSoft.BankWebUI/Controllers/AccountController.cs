using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurkSoft.Services.Interfaces;
using TurkSoft.BankWebUI.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using TurkSoft.Entities.Entities;

namespace TurkSoft.BankWebUI.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;

        public AccountController(
            IUserService userService,
            IUserRoleService userRoleService,
            IRoleService roleService)
        {
            _userService = userService;
            _userRoleService = userRoleService;
            _roleService = roleService;
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

            // Kullanıcının rollerini Claim'e ekle (Authorize(Roles="...") çalışsın)
            var roles = (await _roleService.GetAllRolesAsync())?.ToList() ?? new List<Role>();
            var userRoles = (await _userRoleService.GetAllUserRolesAsync())?.ToList() ?? new List<UserRole>();

            var roleNames = userRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => roles.FirstOrDefault(r => r.Id == ur.RoleId)?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserName", user.UserName ?? user.Email.Split('@')[0])
            };

            foreach (var roleName in roleNames)
                claims.Add(new Claim(ClaimTypes.Role, roleName!));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = vm.RememberMe }
            );

            // Son giriş tarihini güncelle (opsiyonel ama faydalı)
            try
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);
            }
            catch { /* login'i bozma */ }

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
