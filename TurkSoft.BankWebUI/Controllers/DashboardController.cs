using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurkSoft.Business.Base;
using TurkSoft.Entities.Entities;
using TurkSoft.Service.Interface;          // IBankStatementService
using TurkSoft.Services.Interfaces;        // IBankService, IBankAccountService, ISystemLogService

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class DashboardController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankStatementService _bankStatementService;
        private readonly ISystemLogService _systemLogService;

        public DashboardController(
            IBankService bankService,
            IBankAccountService bankAccountService,
            IBankStatementService bankStatementService,
            ISystemLogService systemLogService)
        {
            _bankService = bankService;
            _bankAccountService = bankAccountService;
            _bankStatementService = bankStatementService;
            _systemLogService = systemLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userId = GetUserId();
            var today = DateTime.Today;

            var banks = (await _bankService.GetAllBanksAsync())?.Where(x => x.IsActive).ToList() ?? new();
            var accounts = (await _bankAccountService.GetAllBankAccountsAsync())?.Where(x => x.IsActive).ToList() ?? new();

            decimal dailyIn = 0m;
            decimal dailyOut = 0m;

            // Dashboard’a basit özet: bugün toplam giriş/çıkış
            foreach (var bank in banks)
            {
                var bankAccounts = accounts.Where(a => a.BankId == bank.Id).ToList();

                foreach (var acc in bankAccounts)
                {
                    try
                    {
                        var req = new BankStatementRequest
                        {
                            BankId = bank.ExternalBankId,
                            Username = bank.UsernameLabel,
                            Password = bank.PasswordLabel,
                            AccountNumber = acc.AccountNumber,
                            BeginDate = today,
                            EndDate = today,
                            Link = bank.DefaultLink,
                            TLink = bank.DefaultTLink,
                            Extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "IBAN", acc.IBAN ?? "" },
                                { "SubeNo", acc.SubeNo ?? "" },
                                { "MusteriNo", acc.MusteriNo ?? "" },
                                { "Currency", acc.Currency ?? "" }
                            }
                        };

                        var list = await _bankStatementService.GetStatementAsync(req, ct);

                        foreach (var x in list)
                        {
                            var amt = ToDecimalSafe(x.PROCESSAMAOUNT);
                            if (string.Equals(x.PROCESSDEBORCRED, "C", StringComparison.OrdinalIgnoreCase))
                                dailyIn += amt;
                            else if (string.Equals(x.PROCESSDEBORCRED, "D", StringComparison.OrdinalIgnoreCase))
                                dailyOut += amt;
                        }
                    }
                    catch (Exception ex)
                    {
                        // LOG: IpAddress DB’de NOT NULL -> asla null göndermiyoruz
                        // Ayrıca log yazımı başarısız olursa sayfayı düşürmemeli
                        try
                        {
                            await _systemLogService.CreateSystemLogAsync(new TurkSoft.Entities.Entities.SystemLog
                            {
                                LogLevel = "ERROR",
                                Message = $"Dashboard WS hata: {ex.Message}",
                                Source = "DashboardController",
                                ActionName = "Index",
                                UserId = userId,
                                IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                        catch
                        {
                            // Log yazımı hata fırlatmamalı
                        }
                    }
                }
            }

            ViewBag.DailyIn = dailyIn;
            ViewBag.DailyOut = dailyOut;

            return View(); // mevcut Dashboard view’ı bozma
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
        }

        private static decimal ToDecimalSafe(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            s = s.Trim();

            // TR parse
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("tr-TR"), out var tr))
                return tr;

            // Invariant
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var inv))
                return inv;

            // normalize
            var norm = s.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(norm, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var n2))
                return n2;

            return 0m;
        }
    }
}
