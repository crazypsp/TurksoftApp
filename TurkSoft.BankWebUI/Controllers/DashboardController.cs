using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.BankWebUI.Services;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class DashboardController : Controller
    {
        private readonly IDemoDataService _demo;
        public DashboardController(IDemoDataService demo) => _demo = demo;

        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = "Bankalar arası toplam bakiye, günlük hareket ve mutabakat görünümü";
            return View(_demo.GetDashboard());
        }
    }
}
