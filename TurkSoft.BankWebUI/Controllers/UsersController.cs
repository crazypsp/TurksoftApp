using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Services.Interfaces;
using TurkSoft.Entities.Entities;
using System.Threading.Tasks;
using TurkSoft.BankWebUI.ViewModels;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TurkSoft.BankWebUI.Controllers
{
    // ✅ Sistem kullanıcı yönetimi sadece Admin
    [Authorize(Roles = "Admin")]
    public sealed class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;

        public UsersController(
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService)
        {
            _userService = userService;
            _roleService = roleService;
            _userRoleService = userRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Kullanıcılar";
            ViewData["Subtitle"] = "Sistem kullanıcıları (Admin)";

            var users = (await _userService.GetAllUsersAsync())?.ToList() ?? new List<User>();
            var roles = (await _roleService.GetAllRolesAsync())?.ToList() ?? new List<Role>();
            var userRoles = (await _userRoleService.GetAllUserRolesAsync())?.ToList() ?? new List<UserRole>();

            // Modal dropdown için roller
            ViewBag.Roles = roles.Select(r => r.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // DemoUser formatına dönüştür
            var demoUsers = users.Select(u => new Models.DemoUser
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                Role = GetUserRole(u.Id, userRoles, roles),
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                LastLoginDate = u.LastLoginDate
            }).OrderByDescending(x => x.Id).ToList();

            var model = new UsersIndexVm
            {
                Users = demoUsers
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Kullanıcı Oluştur";
            ViewData["Subtitle"] = "Yeni kullanıcı ekleyin";

            var roles = (await _roleService.GetAllRolesAsync())?.ToList() ?? new List<Role>();
            var vm = new CreateUserVm();

            // DB'de rol yoksa default listeyi kullan, varsa DB listesini göster
            if (roles.Count > 0)
                vm.Roles = roles.Select(r => r.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm vm)
        {
            ViewData["Title"] = "Kullanıcı Oluştur";
            ViewData["Subtitle"] = "Yeni kullanıcı ekleyin";

            var rolesAll = (await _roleService.GetAllRolesAsync())?.ToList() ?? new List<Role>();
            if (rolesAll.Count > 0)
                vm.Roles = rolesAll.Select(r => r.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (!ModelState.IsValid)
                return View(vm);

            if (vm.TempPassword == null || vm.TempPassword.Length < 6)
            {
                ModelState.AddModelError(nameof(vm.TempPassword), "Şifre en az 6 karakter olmalıdır.");
                return View(vm);
            }

            // Email tekil olmalı
            var existing = await _userService.GetUserByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "Bu e-posta ile kayıtlı kullanıcı zaten mevcut.");
                return View(vm);
            }

            // FullName'den FirstName ve LastName ayır
            var nameParts = (vm.FullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            // Şifre hash'leme için salt oluştur
            var salt = Guid.NewGuid().ToString("N");

            var user = new User
            {
                UserName = (vm.Email ?? "").Split('@')[0],
                Email = vm.Email?.Trim() ?? "",
                FirstName = firstName,
                LastName = lastName,
                PasswordSalt = salt,
                PasswordHash = HashPassword(vm.TempPassword, salt),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(user);

            // Rol ID'sini bul (yoksa oluştur)
            var role = rolesAll.FirstOrDefault(r => string.Equals(r.Name, vm.Role, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                role = await _roleService.CreateRoleAsync(new Role
                {
                    Name = vm.Role,
                    Description = $"{vm.Role} rolü",
                    IsSystemRole = true
                });

                // Cache güncelle
                rolesAll.Add(role);
            }

            // Kullanıcı rolünü ekle
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = createdUser.Id,
                    RoleId = role.Id,
                    AssignedDate = DateTime.UtcNow
                };

                await _userRoleService.CreateUserRoleAsync(userRole);
            }

            TempData["Toast"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // Index.cshtml'deki modal form buraya post ediyor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Toast"] = "Kullanıcı güncellenemedi: Eksik/Hatalı alan.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userService.GetUserByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // FullName'den FirstName ve LastName ayır
            var nameParts = (vm.FullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            user.UserName = (vm.Email ?? "").Split('@')[0];
            user.Email = vm.Email?.Trim() ?? "";
            user.FirstName = firstName;
            user.LastName = lastName;
            user.IsActive = vm.IsActive;
            user.ModifiedDate = DateTime.UtcNow;

            await _userService.UpdateUserAsync(user);

            // Rolü güncelle
            var roles = (await _roleService.GetAllRolesAsync())?.ToList() ?? new List<Role>();
            var newRole = roles.FirstOrDefault(r => string.Equals(r.Name, vm.Role, StringComparison.OrdinalIgnoreCase));
            if (newRole == null)
            {
                newRole = await _roleService.CreateRoleAsync(new Role
                {
                    Name = vm.Role,
                    Description = $"{vm.Role} rolü",
                    IsSystemRole = true
                });
                roles.Add(newRole);
            }

            if (newRole != null)
            {
                var userRoles = (await _userRoleService.GetAllUserRolesAsync())?.ToList() ?? new List<UserRole>();
                var existingUserRole = userRoles.FirstOrDefault(ur => ur.UserId == vm.Id);

                if (existingUserRole != null)
                {
                    existingUserRole.RoleId = newRole.Id;
                    await _userRoleService.UpdateUserRoleAsync(existingUserRole);
                }
                else
                {
                    var newUserRole = new UserRole
                    {
                        UserId = vm.Id,
                        RoleId = newRole.Id,
                        AssignedDate = DateTime.UtcNow
                    };
                    await _userRoleService.CreateUserRoleAsync(newUserRole);
                }
            }

            TempData["Toast"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Index.cshtml'deki sil butonu buraya post ediyor.
        // İş kuralı: kullanıcı "silinmez", pasife alınır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (!user.IsActive)
            {
                TempData["Toast"] = "Kullanıcı zaten pasif.";
                return RedirectToAction(nameof(Index));
            }

            await _userService.DeactivateUserAsync(id);
            TempData["Toast"] = "Kullanıcı pasif hale getirildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Toast"] = "Şifre sıfırlanamadı: eksik/hatalı alan.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userService.GetUserByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // Salt boş ise üret (eski kayıtlar için güvenli)
            if (string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                user.PasswordSalt = Guid.NewGuid().ToString("N");
                await _userService.UpdateUserAsync(user);
            }

            await _userService.ResetPasswordAsync(vm.Id, vm.NewPassword);
            TempData["Toast"] = "Şifre başarıyla sıfırlandı.";
            return RedirectToAction(nameof(Index));
        }

        private static string GetUserRole(int userId, IEnumerable<UserRole> userRoles, IEnumerable<Role> roles)
        {
            var userRole = userRoles.FirstOrDefault(ur => ur.UserId == userId);
            if (userRole == null) return "Viewer";

            var role = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
            return role?.Name ?? "Viewer";
        }

        // UserService ile aynı hash (SHA256(password|salt))
        private static string HashPassword(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{password}|{salt}");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
