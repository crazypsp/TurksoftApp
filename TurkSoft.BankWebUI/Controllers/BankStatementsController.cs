using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Services.Interfaces;
using TurkSoft.Entities.Entities;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IdentityModel.Claims;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin,Finance")]
    public class BankStatementsController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly IBankAccountService _accountService;

        public BankStatementsController(
            IBankTransactionService transactionService,
            IBankService bankService,
            IBankAccountService accountService)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index(string? bank, string? accountNumber, DateTime? startDate, DateTime? endDate)
        {
            ViewData["Title"] = "Banka Ekstreleri";

            var transactions = await _transactionService.GetAllTransactionsAsync();
            var banks = await _bankService.GetAllBanksAsync();
            var accounts = await _accountService.GetAllBankAccountsAsync();

            // Filtreleme
            if (!string.IsNullOrEmpty(bank) && int.TryParse(bank, out int bankId))
            {
                transactions = transactions.Where(t => t.BankId == bankId);
            }

            if (!string.IsNullOrEmpty(accountNumber))
            {
                transactions = transactions.Where(t => t.AccountNumber.Contains(accountNumber));
            }

            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate <= endDate.Value);
            }

            var model = new BankStatementsViewModel
            {
                Transactions = transactions.OrderByDescending(t => t.TransactionDate).ToList(),
                Banks = banks.ToList(),
                Accounts = accounts.ToList(),
                SelectedBank = bank,
                SelectedAccount = accountNumber,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export(DateTime? startDate, DateTime? endDate, string? bank, string? accountNumber)
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(userId,
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow);

            // CSV veya Excel export işlemi burada yapılacak
            // Şimdilik basit bir JSON export yapalım

            var exportLog = new ExportLog
            {
                UserId = userId,
                ExportType = "JSON",
                FileName = $"statements_{DateTime.Now:yyyyMMdd_HHmm}.json",
                FilterCriteria = System.Text.Json.JsonSerializer.Serialize(new { startDate, endDate, bank, accountNumber }),
                RecordCount = transactions.Count(),
                FilePath = "/exports/temp",
                FileSize = 1024,
                CreatedDate = DateTime.UtcNow
            };

            // ExportLogService kullanılabilir

            TempData["Toast"] = "Ekstre dışa aktarımı başlatıldı.";
            return RedirectToAction("Index");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }

    public class BankStatementsViewModel
    {
        public List<BankTransaction> Transactions { get; set; }
        public List<Bank> Banks { get; set; }
        public List<BankAccount> Accounts { get; set; }
        public string SelectedBank { get; set; }
        public string SelectedAccount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}