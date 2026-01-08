using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.BankWebUI.Services;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class ReportsController : Controller
    {
        private readonly IDemoDataService _demo;
        public ReportsController(IDemoDataService demo) => _demo = demo;

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Raporlar";
            ViewData["Subtitle"] = "Tarih aralığı ve hesap tipine göre filtreleyin";

            var filter = new ReportFilterVm
            {
                DateRange = $"{DateTime.Today.AddDays(-7):dd.MM.yyyy} - {DateTime.Today:dd.MM.yyyy}"
            };

            return View(_demo.GetReports(filter));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ReportFilterVm filter)
        {
            ViewData["Title"] = "Raporlar";
            ViewData["Subtitle"] = "Tarih aralığı ve hesap tipine göre filtreleyin";
            return View(_demo.GetReports(filter));
        }
    }
}
