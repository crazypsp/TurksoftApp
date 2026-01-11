using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Services.Interfaces;
using TurkSoft.BankWebUI.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class ReportsController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly IExportLogService _exportLogService;

        public ReportsController(
            IBankTransactionService transactionService,
            IBankService bankService,
            IExportLogService exportLogService)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _exportLogService = exportLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Raporlar";
            ViewData["Subtitle"] = "Tarih aralığı ve hesap tipine göre filtreleyin";

            var filter = new ReportFilterVm
            {
                DateRange = $"{DateTime.Today.AddDays(-7):dd.MM.yyyy} - {DateTime.Today:dd.MM.yyyy}"
            };

            return await GetFilteredReports(filter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ReportFilterVm filter)
        {
            ViewData["Title"] = "Raporlar";
            ViewData["Subtitle"] = "Tarih aralığı ve hesap tipine göre filtreleyin";
            return await GetFilteredReports(filter);
        }

        private async Task<IActionResult> GetFilteredReports(ReportFilterVm filter)
        {
            // Tarih aralığını parse et
            DateTime startDate = DateTime.Today.AddDays(-7);
            DateTime endDate = DateTime.Today;

            if (!string.IsNullOrEmpty(filter.DateRange))
            {
                var dates = filter.DateRange.Split(" - ");
                if (dates.Length == 2)
                {
                    DateTime.TryParse(dates[0], out startDate);
                    DateTime.TryParse(dates[1], out endDate);
                }
            }

            var userId = GetCurrentUserId();
            var allTransactions = await _transactionService.GetTransactionsByDateRangeAsync(
                userId, startDate, endDate);

            // Banka filtresi uygula
            var transactions = allTransactions.AsEnumerable();
            if (!string.IsNullOrEmpty(filter.Bank) && int.TryParse(filter.Bank, out int bankId))
            {
                transactions = transactions.Where(t => t.BankId == bankId);
            }

            var banks = await _bankService.GetAllBanksAsync();
            var exportLogs = await _exportLogService.GetAllExportLogsAsync();

            // ViewModel'leri dönüştür
            var reportRows = transactions.Select(t => new Models.BankTransaction
            {
                Id = t.Id,
                Date = t.TransactionDate,
                BankName = t.Bank?.BankName ?? "Bilinmeyen",
                AccountType = GetAccountType(t.AccountNumber),
                ReferenceNo = t.ReferenceNumber,
                Description = t.Description,
                Debit = t.DebitCredit == "D" ? t.Amount : 0,
                Credit = t.DebitCredit == "C" ? t.Amount : 0
            }).ToList();

            // Net değerleri hesapla
            var netByAccountType = reportRows
                .GroupBy(r => r.AccountType)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Net));

            var netByDay = reportRows
                .GroupBy(r => r.Date.Date)
                .ToDictionary(g => g.Key.ToString("dd.MM.yyyy"), g => g.Sum(r => r.Net));

            var model = new ReportsIndexVm
            {
                Filter = filter,
                Rows = reportRows,
                NetByAccountType = netByAccountType,
                NetByDay = netByDay
            };

            // ViewBag'e banka listesini ekle
            ViewBag.Banks = banks.Select(b => new { Value = b.Id.ToString(), Text = b.BankName }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportFilterVm filter, string reportType)
        {
            // Tarih aralığını parse et
            DateTime startDate = DateTime.Today.AddDays(-7);
            DateTime endDate = DateTime.Today;

            if (!string.IsNullOrEmpty(filter.DateRange))
            {
                var dates = filter.DateRange.Split(" - ");
                if (dates.Length == 2)
                {
                    DateTime.TryParse(dates[0], out startDate);
                    DateTime.TryParse(dates[1], out endDate);
                }
            }

            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(
                userId, startDate, endDate);

            // Rapor oluşturma işlemi
            var exportLog = new Entities.Entities.ExportLog
            {
                UserId = userId,
                ExportType = reportType,
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmm}.{reportType.ToLower()}",
                FilterCriteria = System.Text.Json.JsonSerializer.Serialize(filter),
                RecordCount = transactions.Count(),
                FilePath = $"/reports/{userId}/{DateTime.Now:yyyyMMdd}",
                FileSize = 2048,
                CreatedDate = DateTime.UtcNow
            };

            await _exportLogService.CreateExportLogAsync(exportLog);
            TempData["Toast"] = $"{reportType} raporu oluşturuldu ve indirilmeye hazır.";

            return RedirectToAction("Index");
        }

        private string GetAccountType(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber)) return "Diğer";

            // Basit bir hesap tipi belirleme mantığı
            if (accountNumber.StartsWith("1")) return "Vadesiz";
            if (accountNumber.StartsWith("2")) return "Vadeli";
            if (accountNumber.StartsWith("3")) return "Kredi";
            return "Diğer";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}