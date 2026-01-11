using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Services.Interfaces;
using TurkSoft.Entities.Entities;
using System.Threading.Tasks;
using TurkSoft.BankWebUI.ViewModels;
using System.Linq;
using System;

namespace TurkSoft.BankWebUI.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Kullanıcılar";
            ViewData["Subtitle"] = "Sadece Admin rolü erişebilir";

            var users = await _userService.GetAllUsersAsync();
            var roles = await _roleService.GetAllRolesAsync();
            var userRoles = await _userRoleService.GetAllUserRolesAsync();

            // DemoUser formatına dönüştür
            var demoUsers = users.Select(u => new Models.DemoUser
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Role = GetUserRole(u.Id, userRoles, roles),
                IsActive = u.IsActive
            }).ToList();

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

            var roles = await _roleService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.Name).ToList();

            return View(new CreateUserVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm vm)
        {
            ViewData["Title"] = "Kullanıcı Oluştur";
            ViewData["Subtitle"] = "Yeni kullanıcı ekleyin";

            if (!ModelState.IsValid)
            {
                var rolees = await _roleService.GetAllRolesAsync();
                ViewBag.Roles = rolees.Select(r => r.Name).ToList();
                return View(vm);
            }

            // FullName'den FirstName ve LastName ayır
            var nameParts = vm.FullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            // Şifre hash'leme için salt oluştur
            var salt = Guid.NewGuid().ToString();

            var user = new User
            {
                UserName = vm.Email.Split('@')[0], // Email'in @'den önceki kısmını kullan
                Email = vm.Email,
                FirstName = firstName,
                LastName = lastName,
                PasswordSalt = salt,
                PasswordHash = HashPassword(vm.TempPassword, salt),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(user);

            // Rol ID'sini bul
            var roles = await _roleService.GetAllRolesAsync();
            var role = roles.FirstOrDefault(r => r.Name == vm.Role);

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
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (user.IsActive)
            {
                await _userService.DeactivateUserAsync(id);
                TempData["Toast"] = "Kullanıcı pasif hale getirildi.";
            }
            else
            {
                await _userService.ActivateUserAsync(id);
                TempData["Toast"] = "Kullanıcı aktif hale getirildi.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            ViewData["Title"] = "Kullanıcı Düzenle";
            ViewData["Subtitle"] = "Kullanıcı bilgilerini güncelleyin";

            var roles = await _roleService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.Name).ToList();

            // Mevcut rolü bul
            var userRoles = await _userRoleService.GetAllUserRolesAsync();
            var currentUserRole = userRoles.FirstOrDefault(ur => ur.UserId == id);
            var currentRole = currentUserRole != null ?
                roles.FirstOrDefault(r => r.Id == currentUserRole.RoleId)?.Name : "Viewer";

            var vm = new CreateUserVm
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Role = currentRole,
                TempPassword = "" // Şifre gösterilmez
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateUserVm vm)
        {
            if (!ModelState.IsValid)
            {
                var role = await _roleService.GetAllRolesAsync();
                ViewBag.Roles = role.Select(r => r.Name).ToList();
                return View(vm);
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            // FullName'den FirstName ve LastName ayır
            var nameParts = vm.FullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            user.UserName = vm.Email.Split('@')[0];
            user.Email = vm.Email;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.ModifiedDate = DateTime.UtcNow;

            // Şifre değiştirilmişse güncelle
            if (!string.IsNullOrEmpty(vm.TempPassword))
            {
                user.PasswordHash = HashPassword(vm.TempPassword, user.PasswordSalt);
            }

            await _userService.UpdateUserAsync(user);

            // Rolü güncelle
            var roles = await _roleService.GetAllRolesAsync();
            var newRole = roles.FirstOrDefault(r => r.Name == vm.Role);

            if (newRole != null)
            {
                var userRoles = await _userRoleService.GetAllUserRolesAsync();
                var existingUserRole = userRoles.FirstOrDefault(ur => ur.UserId == id);

                if (existingUserRole != null)
                {
                    existingUserRole.RoleId = newRole.Id;
                    await _userRoleService.UpdateUserRoleAsync(existingUserRole);
                }
                else
                {
                    var newUserRole = new UserRole
                    {
                        UserId = id,
                        RoleId = newRole.Id,
                        AssignedDate = DateTime.UtcNow
                    };
                    await _userRoleService.CreateUserRoleAsync(newUserRole);
                }
            }

            TempData["Toast"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        private string GetUserRole(int userId, IEnumerable<UserRole> userRoles, IEnumerable<Role> roles)
        {
            var userRole = userRoles.FirstOrDefault(ur => ur.UserId == userId);
            if (userRole == null) return "Viewer";

            var role = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
            return role?.Name ?? "Viewer";
        }

        private string HashPassword(string password, string salt)
        {
            // UserService'teki hash metodunu kullan
            // Burada basit bir hash yapıyoruz, gerçek uygulamada BCrypt kullanın
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}