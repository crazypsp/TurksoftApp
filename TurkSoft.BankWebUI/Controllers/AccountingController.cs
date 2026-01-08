using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TurkSoft.BankWebUI.Services;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin,Integrator,Finance")]
    public sealed class AccountingController : Controller
    {
        private readonly IDemoDataService _demo;
        public AccountingController(IDemoDataService demo) => _demo = demo;

        [HttpGet]
        public IActionResult Index(string? dateRange, string? bank, string? status)
        {
            ViewData["Title"] = "Aktarım / Muhasebe";
            ViewData["Subtitle"] = "Aktarım listesi, eşleştirme, kuyruk/durum, hata logları";
            return View(_demo.GetAccounting(dateRange, bank, status));
        }

        [HttpGet]
        public IActionResult ExportCsv(string? dateRange, string? bank, string? status)
        {
            var vm = _demo.GetAccounting(dateRange, bank, status);
            var sb = new StringBuilder();
            sb.AppendLine("Id;Date;Bank;AccountType;Reference;Description;Debit;Credit;GLCode;GLName;CostCenter;VendorCode;VendorName;Status");

            foreach (var r in vm.TransferRecords)
            {
                string esc(string? s) => (s ?? "").Replace(";", ",").Replace("\n", " ").Replace("\r", " ");
                sb.AppendLine(string.Join(";",
                    r.Id,
                    r.Date.ToString("yyyy-MM-dd"),
                    esc(r.BankName),
                    esc(r.AccountType),
                    esc(r.ReferenceNo),
                    esc(r.Description),
                    r.Debit.ToString("0.00"),
                    r.Credit.ToString("0.00"),
                    esc(r.GlAccountCode),
                    esc(r.GlAccountName),
                    esc(r.CostCenter),
                    esc(r.VendorCode),
                    esc(r.VendorName),
                    r.Status.ToString()
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet]
        public IActionResult ExportJson(string? dateRange, string? bank, string? status)
        {
            var vm = _demo.GetAccounting(dateRange, bank, status);
            var json = JsonSerializer.Serialize(vm.TransferRecords, new JsonSerializerOptions { WriteIndented = true });
            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8", $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyMapping(int transferId, string glCode, string? costCenter, string? vendorCode)
        {
            _demo.ApplyMapping(transferId, glCode, costCenter, vendorCode);
            TempData["Toast"] = "Eşleştirme kaydedildi (demo).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enqueue(int transferId)
        {
            _demo.EnqueueTransfer(transferId);
            TempData["Toast"] = "Kayıt kuyruğa alındı (demo).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(int queueId)
        {
            _demo.ExportQueueItem(queueId);
            TempData["Toast"] = "Kuyruk kaydı muhasebeye gönderildi (demo).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Retry(int queueId)
        {
            _demo.RetryQueueItem(queueId);
            TempData["Toast"] = "Tekrar deneme kuyruğa alındı (demo).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearError(int errorId)
        {
            _demo.ClearError(errorId);
            TempData["Toast"] = "Hata log kaydı temizlendi (demo).";
            return RedirectToAction("Index");
        }
    }
}
