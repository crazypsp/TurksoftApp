using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TurkSoft.Services.Interfaces;
using TurkSoft.Entities.Entities;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IdentityModel.Claims;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin,Integrator,Finance")]
    public sealed class AccountingController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly ITransferLogService _transferLogService;

        public AccountingController(
            IBankTransactionService transactionService,
            IBankService bankService,
            ITransferLogService transferLogService)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _transferLogService = transferLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? dateRange, string? bank, string? status)
        {
            ViewData["Title"] = "Aktarım / Muhasebe";
            ViewData["Subtitle"] = "Aktarım listesi, eşleştirme, kuyruk/durum, hata logları";

            var transactions = await _transactionService.GetAllTransactionsAsync();
            var banks = await _bankService.GetAllBanksAsync();

            // Filtreleme
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "matched")
                    transactions = transactions.Where(t => t.IsMatched);
                else if (status == "unmatched")
                    transactions = transactions.Where(t => !t.IsMatched);
                else if (status == "transferred")
                    transactions = transactions.Where(t => t.IsTransferred);
            }

            var model = new AccountingViewModel
            {
                TransferRecords = transactions.ToList(),
                Banks = banks.ToList(),
                TransferLogs = (await _transferLogService.GetAllTransferLogsAsync()).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? dateRange, string? bank, string? status)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Date;Bank;AccountNumber;Description;Amount;Currency;Status;Matched;Transferred");

            foreach (var t in transactions)
            {
                string statusText = t.IsTransferred ? "Transferred" : t.IsMatched ? "Matched" : "Unmatched";
                sb.AppendLine(string.Join(";",
                    t.Id,
                    t.TransactionDate.ToString("yyyy-MM-dd"),
                    t.Bank?.BankName ?? "N/A",
                    t.AccountNumber,
                    t.Description?.Replace(";", ",") ?? "",
                    t.Amount.ToString("0.00"),
                    t.Currency,
                    statusText,
                    t.IsMatched.ToString(),
                    t.IsTransferred.ToString()
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportJson(string? dateRange, string? bank, string? status)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8", $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyMapping(int transactionId, string clCardCode, string clCardName)
        {
            var userId = GetCurrentUserId();
            var result = await _transactionService.MatchTransactionAsync(transactionId, clCardCode, clCardName, userId);

            if (result != null)
                TempData["Toast"] = "Hareket başarıyla eşleştirildi.";
            else
                TempData["Toast"] = "Eşleştirme başarısız.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _transactionService.TransferTransactionAsync(transactionId, userId);

            if (transaction != null)
            {
                // Transfer log kaydı oluştur
                var transferLog = new TransferLog
                {
                    UserId = userId,
                    TransactionId = transactionId,
                    TransferType = "SINGLE",
                    Status = "SUCCESS",
                    TargetSystem = "LOGO_TIGER",
                    RequestData = JsonSerializer.Serialize(new { TransactionId = transactionId }),
                    ResponseData = "Transfer başarılı",
                    CreatedDate = DateTime.UtcNow,
                    CompletedDate = DateTime.UtcNow
                };

                await _transferLogService.CreateTransferLogAsync(transferLog);
                TempData["Toast"] = "Hareket başarıyla muhasebeye aktarıldı.";
            }
            else
            {
                TempData["Toast"] = "Aktarım başarısız. Önce eşleştirme yapılmalı.";
            }

            return RedirectToAction("Index");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }

    public class AccountingViewModel
    {
        public List<BankTransaction> TransferRecords { get; set; }
        public List<Bank> Banks { get; set; }
        public List<TransferLog> TransferLogs { get; set; }
    }
}