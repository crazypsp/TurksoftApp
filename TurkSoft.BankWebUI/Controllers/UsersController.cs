using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.BankWebUI.Services;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public sealed class UsersController : Controller
    {
        private readonly IDemoDataService _demo;
        public UsersController(IDemoDataService demo) => _demo = demo;

        public IActionResult Index()
        {
            ViewData["Title"] = "Kullanıcılar";
            ViewData["Subtitle"] = "Sadece Admin rolü erişebilir";
            return View(_demo.GetUsers());
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Kullanıcı Oluştur";
            ViewData["Subtitle"] = "Demo kullanıcı ekler";
            return View(new CreateUserVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserVm vm)
        {
            ViewData["Title"] = "Kullanıcı Oluştur";
            ViewData["Subtitle"] = "Demo kullanıcı ekler";

            if (!ModelState.IsValid) return View(vm);

            _demo.AddUser(vm);
            TempData["Toast"] = "Kullanıcı oluşturuldu (demo).";
            return RedirectToAction("Index");
        }
    }
}
