using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TurkSoft.BankWebUI.Models;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Services.Interfaces;

// 🔥 Ambiguity fix: Entity tiplerine alias veriyoruz
using EntityBankTransaction = TurkSoft.Entities.Entities.BankTransaction;
using EntityBank = TurkSoft.Entities.Entities.Bank;
using EntityTransferLog = TurkSoft.Entities.Entities.TransferLog;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
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

        // ---------------------------
        // INDEX
        // ---------------------------
        [HttpGet]
        public async Task<IActionResult> Index(string? dateRange, string? bank, string? status)
        {
            var (from, to) = ParseDateRange(dateRange);

            // Servislerden veri çek
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var banks = await _bankService.GetAllBanksAsync();

            // Null gelme ihtimaline karşı güvenli hale getir
            var txList = transactions ?? new List<EntityBankTransaction>();
            var bankList = banks ?? new List<EntityBank>();

            // Filtreleme (transaction seviyesinde)
            IEnumerable<EntityBankTransaction> filtered = txList;

            if (from.HasValue)
                filtered = filtered.Where(t => t.TransactionDate.Date >= from.Value.Date);

            if (to.HasValue)
                filtered = filtered.Where(t => t.TransactionDate.Date <= to.Value.Date);

            if (!string.IsNullOrWhiteSpace(bank))
                filtered = filtered.Where(t => (t.Bank?.BankName ?? "").Equals(bank, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
            {
                // View'daki status değerleri: Draft, Ready, Queued, Exported
                filtered = status switch
                {
                    "Draft" => filtered.Where(t => !t.IsMatched && !t.IsTransferred),
                    "Ready" => filtered.Where(t => t.IsMatched && !t.IsTransferred),
                    "Exported" => filtered.Where(t => t.IsTransferred),
                    "Queued" => filtered.Where(_ => false), // Queue altyapısı yoksa boş
                    _ => filtered
                };
            }

            // EntityBankTransaction -> TransferRecord (UI model)
            var records = filtered
                .Select(MapToTransferRecord)
                .OrderByDescending(x => x.Date)
                .ToList();

            // Bank isimleri dropdown
            var bankNames = bankList
                .Select(b => b.BankName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var vm = new AccountingVm
            {
                DateRange = dateRange,
                Bank = bank,
                Status = status,

                Banks = bankNames,
                TransferRecords = records,

                // Bu alanlar sende var ama servis yoksa boş kalabilir (View patlamaz)
                Mappings = new List<AccountMapping>(),
                GlAccounts = new List<GlAccount>(),
                Vendors = new List<Vendor>(),
                Queue = new List<TransferQueueItem>(),
                Errors = new List<TransferErrorLog>()
            };

            return View(vm);
        }

        // ---------------------------
        // EXPORTS
        // ---------------------------
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? dateRange, string? bank, string? status)
        {
            var records = await GetFilteredRecords(dateRange, bank, status);

            var sb = new StringBuilder();
            sb.AppendLine("Id;Date;Bank;Description;Debit;Credit;Status");

            foreach (var r in records)
            {
                sb.AppendLine(string.Join(";",
                    r.Id,
                    r.Date.ToString("yyyy-MM-dd"),
                    SafeCsv(r.BankName),
                    SafeCsv(r.Description),
                    r.Debit.ToString("0.00", CultureInfo.InvariantCulture),
                    r.Credit.ToString("0.00", CultureInfo.InvariantCulture),
                    r.Status.ToString()
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportJson(string? dateRange, string? bank, string? status)
        {
            var records = await GetFilteredRecords(dateRange, bank, status);
            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });

            return File(Encoding.UTF8.GetBytes(json),
                "application/json; charset=utf-8",
                $"accounting_export_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        // ---------------------------
        // VIEW'DAN GELEN POST ACTIONLAR
        // ---------------------------

        // View: asp-action="ApplyMapping"
        // View form field names: transferId, glCode, costCenter, vendorCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyMapping(int transferId, string? glCode, string? costCenter, string? vendorCode)
        {
            var userId = GetCurrentUserId();

            // Senin servisin şu imzayı bekliyor:
            // MatchTransactionAsync(transactionId, clCardCode, clCardName, userId)
            // View sadece vendorCode yolluyor, name yok → boş geçiyoruz
            var clCardCode = (vendorCode ?? "").Trim();
            var clCardName = ""; // View tarafında name gönderilmiyor

            if (string.IsNullOrWhiteSpace(clCardCode))
            {
                TempData["Toast"] = "Eşleştirme başarısız: Tedarikçi/Cari kodu boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _transactionService.MatchTransactionAsync(transferId, clCardCode, clCardName, userId);

            TempData["Toast"] = result != null
                ? "Hareket başarıyla eşleştirildi."
                : "Eşleştirme başarısız.";

            return RedirectToAction(nameof(Index));
        }

        // View: asp-action="Enqueue"  (Ready olanlarda)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enqueue(int transferId)
        {
            return await DoTransfer(transferId);
        }

        // View: Retry/Export (Queue paneli)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Retry(int queueId)
        {
            TempData["Toast"] = "Retry: Kuyruk altyapısı aktif değil.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(int queueId)
        {
            TempData["Toast"] = "Export: Kuyruk altyapısı aktif değil.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // INTERNAL
        // ---------------------------

        private async Task<IActionResult> DoTransfer(int transactionId)
        {
            var userId = GetCurrentUserId();

            var transaction = await _transactionService.TransferTransactionAsync(transactionId, userId);

            if (transaction != null)
            {
                var log = new EntityTransferLog
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

                await _transferLogService.CreateTransferLogAsync(log);
                TempData["Toast"] = "Hareket başarıyla muhasebeye aktarıldı.";
            }
            else
            {
                TempData["Toast"] = "Aktarım başarısız. Önce eşleştirme yapılmalı.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static TransferRecord MapToTransferRecord(EntityBankTransaction t)
        {
            var amount = t.Amount;

            // Basit debit/credit ayrımı
            var debit = amount < 0 ? Math.Abs(amount) : 0m;
            var credit = amount > 0 ? amount : 0m;

            var status =
                t.IsTransferred ? TransferRecordStatus.Exported
                : t.IsMatched ? TransferRecordStatus.Ready
                : TransferRecordStatus.Draft;

            // 🔴 IsMapped read-only olduğu için SET ETMİYORUZ (CS0200 fix)
            return new TransferRecord
            {
                Id = t.Id,
                Date = t.TransactionDate,
                BankName = t.Bank?.BankName ?? "N/A",
                Description = t.Description ?? "",
                Debit = debit,
                Credit = credit,
                Status = status
            };
        }

        private async Task<List<TransferRecord>> GetFilteredRecords(string? dateRange, string? bank, string? status)
        {
            var (from, to) = ParseDateRange(dateRange);

            var transactions = await _transactionService.GetAllTransactionsAsync();
            var txList = transactions ?? new List<EntityBankTransaction>();

            IEnumerable<EntityBankTransaction> filtered = txList;

            if (from.HasValue)
                filtered = filtered.Where(t => t.TransactionDate.Date >= from.Value.Date);

            if (to.HasValue)
                filtered = filtered.Where(t => t.TransactionDate.Date <= to.Value.Date);

            if (!string.IsNullOrWhiteSpace(bank))
                filtered = filtered.Where(t => (t.Bank?.BankName ?? "").Equals(bank, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
            {
                filtered = status switch
                {
                    "Draft" => filtered.Where(t => !t.IsMatched && !t.IsTransferred),
                    "Ready" => filtered.Where(t => t.IsMatched && !t.IsTransferred),
                    "Exported" => filtered.Where(t => t.IsTransferred),
                    "Queued" => filtered.Where(_ => false),
                    _ => filtered
                };
            }

            return filtered.Select(MapToTransferRecord).OrderByDescending(x => x.Date).ToList();
        }

        private static (DateTime? from, DateTime? to) ParseDateRange(string? dateRange)
        {
            if (string.IsNullOrWhiteSpace(dateRange))
                return (null, null);

            // flatpickr range bazen "01.01.2026 to 15.01.2026", bazen "01.01.2026 - 15.01.2026"
            var s = dateRange.Trim();

            string[] parts = s.Contains(" to ", StringComparison.OrdinalIgnoreCase)
                ? s.Split(" to ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : s.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var tr = new CultureInfo("tr-TR");

            DateTime? from = TryParseTr(parts.ElementAtOrDefault(0), tr);
            DateTime? to = TryParseTr(parts.ElementAtOrDefault(1), tr);

            return (from, to);
        }

        private static DateTime? TryParseTr(string? input, CultureInfo tr)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            if (DateTime.TryParse(input, tr, DateTimeStyles.AssumeLocal, out var dt))
                return dt;

            if (DateTime.TryParseExact(input, new[] { "d.M.yyyy", "dd.MM.yyyy" }, tr,
                DateTimeStyles.AssumeLocal, out dt))
                return dt;

            return null;
        }

        private static string SafeCsv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace(";", ",").Replace("\r", " ").Replace("\n", " ");
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
        }
    }
}
