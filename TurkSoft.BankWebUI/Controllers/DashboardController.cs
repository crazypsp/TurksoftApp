using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Entities.Entities;
using TurkSoft.Service.Interface;
using TurkSoft.Services.Interfaces;

// ✅ Ambiguous fix: UI modellerine alias
using UiBankBalance = TurkSoft.BankWebUI.Models.BankBalance;
using UiBankTransaction = TurkSoft.BankWebUI.Models.BankTransaction;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class DashboardController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankCredentialService _bankCredentialService;
        private readonly IBankStatementService _bankStatementService;
        private readonly ISystemLogService _systemLogService;

        public DashboardController(
            IBankService bankService,
            IBankAccountService bankAccountService,
            IBankCredentialService bankCredentialService,
            IBankStatementService bankStatementService,
            ISystemLogService systemLogService)
        {
            _bankService = bankService;
            _bankAccountService = bankAccountService;
            _bankCredentialService = bankCredentialService;
            _bankStatementService = bankStatementService;
            _systemLogService = systemLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = "Bankalar arası toplam bakiye, günlük hareket ve mutabakat görünümü";

            var userId = GetUserId();
            var today = DateTime.Today;
            var from7 = today.AddDays(-6);

            var vm = new DashboardVm
            {
                BankBalances = new List<UiBankBalance>(),
                LatestTransactions = new List<UiBankTransaction>(),
                CashflowLabels = new List<string>(),
                CashflowNet = new List<decimal>()
            };

            try
            {
                var banks = (await _bankService.GetAllBanksAsync())
                    ?.Where(b => b.IsActive)
                    .ToList() ?? new List<Bank>();

                var accounts = (await _bankAccountService.GetAllBankAccountsAsync())
                    ?.Where(a => a.IsActive)
                    .ToList() ?? new List<BankAccount>();

                var creds = (await _bankCredentialService.GetAllCredentialsAsync())
                    ?.Where(c => c.UserId == userId)
                    .ToList() ?? new List<BankCredential>();

                var all = new List<(Bank bank, BankAccount acc, BNKHAR row)>();

                foreach (var bank in banks)
                {
                    var cred = creds.FirstOrDefault(c => c.BankId == bank.Id);
                    if (cred == null) continue;

                    var bankAccounts = accounts.Where(a => a.BankId == bank.Id).ToList();
                    if (bankAccounts.Count == 0) continue;

                    var credExtras = ParseExtras(cred.Extras);

                    foreach (var acc in bankAccounts)
                    {
                        var req = new BankStatementRequest
                        {
                            BankId = bank.ExternalBankId,
                            Username = bank.UsernameLabel,
                            Password = bank.PasswordLabel,
                            AccountNumber = acc.AccountNumber,
                            BeginDate = from7,
                            EndDate = today,
                            Link = bank.DefaultLink,
                            TLink = bank.DefaultTLink,
                            Extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        };

                        req.Extras["IBAN"] = acc.IBAN ?? "";
                        req.Extras["SubeNo"] = acc.SubeNo ?? "";
                        req.Extras["MusteriNo"] = acc.MusteriNo ?? "";
                        req.Extras["Currency"] = acc.Currency ?? "";

                        foreach (var kv in credExtras)
                            req.Extras[kv.Key] = kv.Value ?? "";

                        IReadOnlyList<BNKHAR> rows;
                        try
                        {
                            rows = await _bankStatementService.GetStatementAsync(req, ct);
                        }
                        catch (Exception ex)
                        {
                            SafeLog(userId, "DashboardController", "Index",
                                $"WS hata. Bank={bank.BankName}, Acc={acc.AccountNumber}. {ex.Message}");
                            continue;
                        }

                        foreach (var r in rows)
                            all.Add((bank, acc, r));
                    }
                }

                // Banka bakiyeleri
                var bankBalances = all
                    .GroupBy(x => x.bank.Id)
                    .Select(g =>
                    {
                        var bank = g.First().bank;

                        var last = g
                            .OrderByDescending(x => GetRowDate(x.row) ?? DateTime.MinValue)
                            .FirstOrDefault();

                        decimal balTry;
                        if (!string.IsNullOrWhiteSpace(last.row?.PROCESSBALANCE))
                            balTry = ToDecimalSafe(last.row.PROCESSBALANCE);
                        else
                            balTry = g.Sum(x => GetSignedAmount(x.row));

                        return new UiBankBalance
                        {
                            BankName = bank.BankName,
                            BalanceTry = balTry
                        };
                    })
                    .OrderByDescending(x => x.BalanceTry)
                    .ToList();

                vm.BankBalances = bankBalances;
                vm.TotalBalance = bankBalances.Sum(x => x.BalanceTry);

                // Günlük giriş / çıkış
                var todays = all
                    .Where(x => (GetRowDate(x.row)?.Date) == today)
                    .Select(x => x.row)
                    .ToList();

                vm.DailyIn = todays.Where(IsCredit).Sum(r => Math.Abs(ToDecimalSafe(r.PROCESSAMAOUNT)));
                vm.DailyOut = todays.Where(IsDebit).Sum(r => Math.Abs(ToDecimalSafe(r.PROCESSAMAOUNT)));

                // Cashflow (son 7 gün)
                for (int i = 0; i < 7; i++)
                {
                    var d = from7.AddDays(i);
                    vm.CashflowLabels.Add(d.ToString("dd.MM"));

                    var net = all
                        .Where(x => (GetRowDate(x.row)?.Date) == d.Date)
                        .Sum(x => GetSignedAmount(x.row));

                    vm.CashflowNet.Add(net);
                }

                // ✅ Son hareketler -> UI tipi ile
                vm.LatestTransactions = all
                    .Select(x => new UiBankTransaction
                    {
                        Id = x.row.Id,
                        Date = GetRowDate(x.row) ?? DateTime.MinValue,
                        BankName = x.bank.BankName,
                        AccountType = x.acc.Currency ?? (x.row.CURRENCYCODE ?? "TRY"),
                        ReferenceNo = x.row.PROCESSREFNO ?? x.row.REFNO ?? "",
                        Description = BuildDesc(x.row),
                        Debit = IsDebit(x.row) ? Math.Abs(ToDecimalSafe(x.row.PROCESSAMAOUNT)) : 0m,
                        Credit = IsCredit(x.row) ? Math.Abs(ToDecimalSafe(x.row.PROCESSAMAOUNT)) : 0m
                    })
                    .OrderByDescending(t => t.Date)
                    .Take(20)
                    .ToList();

                vm.ReconcileAlerts = 0;

                return View(vm);
            }
            catch (Exception ex)
            {
                SafeLog(userId, "DashboardController", "Index", $"Dashboard genel hata: {ex.Message}");
                return View(vm);
            }
        }

        // ---------------- Helpers ----------------

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
        }

        private void SafeLog(int? userId, string source, string action, string message)
        {
            try
            {
                _systemLogService.CreateSystemLogAsync(new SystemLog
                {
                    LogLevel = "ERROR",
                    Message = message,
                    Source = source,
                    ActionName = action,
                    UserId = userId,
                    IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    CreatedDate = DateTime.UtcNow
                }).GetAwaiter().GetResult();
            }
            catch { }
        }

        private static Dictionary<string, string> ParseExtras(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var d = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return d ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static DateTime? GetRowDate(BNKHAR r)
        {
            if (r.PROCESSTIME.HasValue) return r.PROCESSTIME;

            var s = (r.PROCESSTIMESTR ?? "").Trim();
            if (DateTime.TryParse(s, new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out var dtTr))
                return dtTr;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtInv))
                return dtInv;

            return null;
        }

        private static string BuildDesc(BNKHAR r)
        {
            var parts = new[]
            {
                r.PROCESSDESC,
                r.PROCESSDESC2,
                r.PROCESSDESC3,
                r.PROCESSDESC4
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());

            return string.Join(" ", parts);
        }

        // Projende Normalize: A = Credit, B = Debit
        private static bool IsCredit(BNKHAR r)
            => string.Equals(r.PROCESSDEBORCRED, "A", StringComparison.OrdinalIgnoreCase);

        private static bool IsDebit(BNKHAR r)
            => string.Equals(r.PROCESSDEBORCRED, "B", StringComparison.OrdinalIgnoreCase);

        private static decimal GetSignedAmount(BNKHAR r)
        {
            var amt = Math.Abs(ToDecimalSafe(r.PROCESSAMAOUNT));
            return IsCredit(r) ? amt : -amt;
        }

        private static decimal ToDecimalSafe(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            s = s.Trim();

            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("tr-TR"), out var tr))
                return tr;

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
                return inv;

            var norm = s.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(norm, NumberStyles.Any, CultureInfo.InvariantCulture, out var n2))
                return n2;

            return 0m;
        }
    }
}
