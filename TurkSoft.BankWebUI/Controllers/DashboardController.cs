using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System;
using TurkSoft.Entities.Entities;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class DashboardController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly IUserService _userService;
        private readonly ITransactionImportService _importService;

        public DashboardController(
            IBankTransactionService transactionService,
            IBankService bankService,
            IUserService userService,
            ITransactionImportService importService)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _userService = userService;
            _importService = importService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = "Bankalar arası toplam bakiye, günlük hareket ve mutabakat görünümü";

            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var banks = await _bankService.GetAllBanksAsync();
            var users = await _userService.GetAllUsersAsync();
            var imports = await _importService.GetAllImportsAsync();

            var today = DateTime.UtcNow.Date;
            var recentTransactions = transactions
                .Where(t => t.TransactionDate >= today.AddDays(-7))
                .ToList();

            // Banka bakiyelerini hesapla
            var bankBalances = banks.Select(b => new Models.BankBalance
            {
                BankName = b.BankName,
                BalanceTry = transactions
                    .Where(t => t.BankId == b.Id && t.Currency == "TRY")
                    .Sum(t => t.Amount * (t.DebitCredit == "C" ? 1 : -1))
            }).ToList();

            // Son 30 günlük cashflow verisi
            var cashflowLabels = Enumerable.Range(0, 30)
                .Select(i => today.AddDays(-29 + i).ToString("dd.MM"))
                .ToList();

            var cashflowNet = cashflowLabels.Select((label, index) =>
            {
                var date = today.AddDays(-29 + index);
                var dayTransactions = transactions.Where(t => t.TransactionDate.Date == date);
                return dayTransactions.Sum(t => t.Amount * (t.DebitCredit == "C" ? 1 : -1));
            }).ToList();

            var model = new DashboardVm
            {
                TotalBalance = bankBalances.Sum(b => b.BalanceTry),
                DailyIn = transactions
                    .Where(t => t.TransactionDate.Date == today && t.DebitCredit == "C")
                    .Sum(t => t.Amount),
                DailyOut = transactions
                    .Where(t => t.TransactionDate.Date == today && t.DebitCredit == "D")
                    .Sum(t => t.Amount),
                ReconcileAlerts = transactions.Count(t => !t.IsMatched),
                BankBalances = bankBalances,
                LatestTransactions = recentTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new Models.BankTransaction
                    {
                        Id = t.Id,
                        Date = t.TransactionDate,
                        BankName = banks.FirstOrDefault(b => b.Id == t.BankId)?.BankName ?? "Bilinmeyen",
                        AccountType = GetAccountType(t.AccountNumber),
                        ReferenceNo = t.ReferenceNumber,
                        Description = t.Description,
                        Debit = t.DebitCredit == "D" ? t.Amount : 0,
                        Credit = t.DebitCredit == "C" ? t.Amount : 0
                    })
                    .ToList(),
                CashflowLabels = cashflowLabels,
                CashflowNet = cashflowNet
            };

            return View(model);
        }

        private string GetAccountType(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber)) return "Diğer";
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