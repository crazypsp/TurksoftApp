using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.BankWebUI.Services;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin,Integrator,Finance")]
    public sealed class BankIntegrationsController : Controller
    {
        private readonly IDemoDataService _demo;
        public BankIntegrationsController(IDemoDataService demo) => _demo = demo;

        public IActionResult Index()
        {
            ViewData["Title"] = "Banka Entegrasyonları";
            ViewData["Subtitle"] = "Bağlantı ayarları, çekim planı, son çekim ve durum";
            return View(_demo.GetBankIntegrations());
        }

        public IActionResult Logs(string? bank)
        {
            ViewData["Title"] = "Çekim Logları";
            ViewData["Subtitle"] = "Bankadan çekim denemeleri ve sonuçları (demo)";
            return View(_demo.GetBankPullLogs(bank));
        }

        public IActionResult Delta(int logId)
        {
            var vm = _demo.GetDeltaByLogId(logId);
            ViewData["Title"] = "Son Çekimden Beri Gelenler";
            ViewData["Subtitle"] = $"{vm.BankName} · {vm.FromAt:dd.MM.yyyy HH:mm} → {vm.ToAt:dd.MM.yyyy HH:mm}";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(string id)
        {
            _demo.ToggleBankActive(id);
            TempData["Toast"] = "Entegrasyon durumu güncellendi (demo).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PullNow(string id)
        {
            _demo.PullNow(id);
            TempData["Toast"] = "Banka çekimi çalıştırıldı (demo).";
            return RedirectToAction("Index");
        }
    }
}
