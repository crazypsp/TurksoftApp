using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Entities.Entities;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;    // ILogoTigerIntegrationService
using TurkSoft.Service.Interface;     // IBankStatementService
using TurkSoft.Services.Interfaces;   // IBankService, IBankAccountService, ITransferLogService, ISystemLogService

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class AccountingController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankStatementService _bankStatementService;
        private readonly ILogoTigerIntegrationService _logoTiger;
        private readonly ITransferLogService _transferLogService;
        private readonly ISystemLogService _systemLogService;

        public AccountingController(
            IBankService bankService,
            IBankAccountService bankAccountService,
            IBankStatementService bankStatementService,
            ILogoTigerIntegrationService logoTiger,
            ITransferLogService transferLogService,
            ISystemLogService systemLogService)
        {
            _bankService = bankService;
            _bankAccountService = bankAccountService;
            _bankStatementService = bankStatementService;
            _logoTiger = logoTiger;
            _transferLogService = transferLogService;
            _systemLogService = systemLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var banks = (await _bankService.GetAllBanksAsync())?.Where(x => x.IsActive).ToList() ?? new();
            var accounts = (await _bankAccountService.GetAllBankAccountsAsync())?.Where(x => x.IsActive).ToList() ?? new();

            ViewBag.Banks = banks;
            ViewBag.Accounts = accounts;
            return View();
        }

        // WS’den getir + TransferLog’a göre işaretle (BNKHAR’a dokunmadan DTO döndür)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBankStatement([FromForm] int bankId, [FromForm] int accountId, [FromForm] string dateRange, CancellationToken ct)
        {
            var userId = GetUserId();

            try
            {
                var bank = await _bankService.GetBankByIdAsync(bankId);
                var acc = await _bankAccountService.GetBankAccountByIdAsync(accountId);

                if (bank == null || acc == null || acc.BankId != bankId)
                    return Json(new { success = false, message = "Banka/Hesap bulunamadı." });

                var (start, end) = ParseDateRange(dateRange);

                var req = new BankStatementRequest
                {
                    BankId = bank.ExternalBankId,
                    Username = bank.UsernameLabel,
                    Password = bank.PasswordLabel,
                    AccountNumber = acc.AccountNumber,
                    BeginDate = start,
                    EndDate = end,
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

                var list = (await _bankStatementService.GetStatementAsync(req, ct))?.ToList() ?? new();

                var logs = (await _transferLogService.GetAllTransferLogsAsync())?.ToList() ?? new();
                var transferred = new HashSet<string>(
                    logs.Where(l => l.Status == "SUCCESS" && !string.IsNullOrWhiteSpace(l.ExternalUniqueKey))
                        .Select(l => l.ExternalUniqueKey),
                    StringComparer.OrdinalIgnoreCase);

                var dto = new List<BankStatementRowDto>();

                foreach (var x in list)
                {
                    var key = BuildUniqueKey(bankId, acc.AccountNumber, x);
                    var log = logs.FirstOrDefault(l => l.ExternalUniqueKey == key && l.Status == "SUCCESS");

                    dto.Add(new BankStatementRowDto
                    {
                        ProcessTimeStr = x.PROCESSTIMESTR,
                        ProcessTime = x.PROCESSTIME,
                        RefNo = x.PROCESSREFNO,
                        Description = x.PROCESSDESC,
                        AmountStr = x.PROCESSAMAOUNT,
                        DebitCredit = x.PROCESSDEBORCRED,
                        TypeCode = x.PROCESSTYPECODE,
                        ExternalUniqueKey = key,
                        IsTransferred = transferred.Contains(key),
                        TigerFicheRef = log?.TigerFicheRef
                    });
                }

                return Json(new
                {
                    success = true,
                    data = dto,
                    bankInfo = new { bankName = bank.BankName, accountNumber = acc.AccountNumber }
                });
            }
            catch (Exception ex)
            {
                // LOG: IpAddress DB’de NOT NULL -> asla null göndermiyoruz
                // Ayrıca log yazımı başarısız olursa action’ı düşürmemeli
                try
                {
                    await _systemLogService.CreateSystemLogAsync(new SystemLog
                    {
                        LogLevel = "ERROR",
                        Message = $"Accounting GetBankStatement hata: {ex.Message}",
                        Source = "AccountingController",
                        ActionName = "GetBankStatement",
                        UserId = userId,
                        IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                        CreatedDate = DateTime.UtcNow
                    });
                }
                catch
                {
                    // Log yazımı hata fırlatmamalı
                }

                return Json(new { success = false, message = ex.Message });
            }
        }

        // Transfer: sonucu normalize ediyoruz (Kredi/BankaFis dönüş tipi farkını kaldırıyoruz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToTiger([FromBody] TransferToTigerRequest dto)
        {
            var userId = GetUserId();

            var logs = (await _transferLogService.GetAllTransferLogsAsync())?.ToList() ?? new();
            if (logs.Any(l => l.ExternalUniqueKey == dto.ExternalUniqueKey && l.Status == "SUCCESS"))
                return Json(new { success = true, alreadyTransferred = true, message = "Bu kayıt daha önce aktarılmış." });

            bool ok;
            string msg;
            int? ficheRef = null;
            List<string>? errors = null;

            try
            {
                if (IsKredi(dto.Description, dto.TypeCode))
                {
                    var req = BuildKredi(dto, userId);
                    var res = await _logoTiger.KrediTaksitOdemeEkleAsync(req);
                    ok = res.Success; msg = res.Message; ficheRef = res.Data?.FisReferans; errors = res.Errors;
                }
                else if (IsVirman(dto.Description, dto.TypeCode))
                {
                    var req = BuildVirman(dto, userId);
                    var res = await _logoTiger.VirmanEkleAsync(req);
                    ok = res.Success; msg = res.Message; ficheRef = res.Data?.FisReferans; errors = res.Errors;
                }
                else if (string.Equals(dto.DebitCredit, "C", StringComparison.OrdinalIgnoreCase))
                {
                    var req = BuildGelen(dto, userId);
                    var res = await _logoTiger.GelenHavaleEkleAsync(req);
                    ok = res.Success; msg = res.Message; ficheRef = res.Data?.FisReferans; errors = res.Errors;
                }
                else
                {
                    var req = BuildGiden(dto, userId);
                    var res = await _logoTiger.GidenHavaleEkleAsync(req);
                    ok = res.Success; msg = res.Message; ficheRef = res.Data?.FisReferans; errors = res.Errors;
                }

                await _transferLogService.CreateTransferLogAsync(new TransferLog
                {
                    UserId = userId,
                    TransferType = "SINGLE",
                    TargetSystem = "LOGO_TIGER",
                    Status = ok ? "SUCCESS" : "FAILED",
                    ExternalUniqueKey = dto.ExternalUniqueKey,
                    RequestData = JsonSerializer.Serialize(dto),
                    ResponseData = ok ? (ficheRef?.ToString() ?? "") : (errors != null ? string.Join(" | ", errors) : msg),
                    ErrorMessage = ok ? null : (errors != null ? string.Join(" | ", errors) : msg),
                    TigerFicheRef = ficheRef,
                    CreatedDate = DateTime.UtcNow,
                    CompletedDate = DateTime.UtcNow
                });

                return Json(new { success = ok, message = msg, ficheRef, errors });
            }
            catch (Exception ex)
            {
                await _transferLogService.CreateTransferLogAsync(new TransferLog
                {
                    UserId = userId,
                    TransferType = "SINGLE",
                    TargetSystem = "LOGO_TIGER",
                    Status = "FAILED",
                    ExternalUniqueKey = dto.ExternalUniqueKey,
                    RequestData = JsonSerializer.Serialize(dto),
                    ErrorMessage = ex.Message,
                    CreatedDate = DateTime.UtcNow,
                    CompletedDate = DateTime.UtcNow
                });

                return Json(new { success = false, message = ex.Message });
            }
        }

        // -------- DTO / helpers --------

        public sealed class BankStatementRowDto
        {
            public string? ProcessTimeStr { get; set; }
            public DateTime? ProcessTime { get; set; }
            public string? RefNo { get; set; }
            public string? Description { get; set; }
            public string? AmountStr { get; set; }
            public string? DebitCredit { get; set; }
            public string? TypeCode { get; set; }

            public string ExternalUniqueKey { get; set; } = "";
            public bool IsTransferred { get; set; }
            public int? TigerFicheRef { get; set; }
        }

        public sealed class TransferToTigerRequest
        {
            public string ExternalUniqueKey { get; set; } = "";
            public DateTime TransactionDate { get; set; }
            public string? DebitCredit { get; set; } // D/C
            public decimal Amount { get; set; }

            public string? RefNo { get; set; }
            public string? Description { get; set; }
            public string? TypeCode { get; set; }

            // tiger mapping (projende dolduracağın alanlar)
            public string? BankaHesapKodu { get; set; }
            public string? HedefHesapKodu { get; set; } // virman/kredi
            public string? ArpCode { get; set; }
            public string? FisNo { get; set; }
            public int DataReference { get; set; }
        }

        private static (DateTime start, DateTime end) ParseDateRange(string dateRange)
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(dateRange))
            {
                var parts = dateRange.Split(" - ");
                if (parts.Length == 2)
                {
                    if (DateTime.TryParse(parts[0], out var s)) start = s;
                    if (DateTime.TryParse(parts[1], out var e)) end = e;
                }
            }
            return (start, end);
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
        }

        private static string BuildUniqueKey(int bankId, string accountNo, BNKHAR x)
        {
            var raw = $"{bankId}|{accountNo}|{x.PROCESSTIMESTR}|{x.PROCESSREFNO}|{x.PROCESSAMAOUNT}|{x.PROCESSDEBORCRED}|{x.PROCESSDESC}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw ?? ""));
            return Convert.ToHexString(bytes);
        }

        private static bool IsVirman(string? desc, string? typeCode)
            => (desc ?? "").ToUpper().Contains("VIRMAN") || (typeCode ?? "").ToUpper().Contains("VIR");

        private static bool IsKredi(string? desc, string? typeCode)
            => (desc ?? "").ToUpper().Contains("KREDI") || (desc ?? "").ToUpper().Contains("TAKSIT") || (typeCode ?? "").ToUpper().Contains("KRD");

        // ---- Build Requests (minimum alanlarla) ----
        private static string NewGuidUpper() => Guid.NewGuid().ToString().ToUpperInvariant();

        private static GelenHavaleRequest BuildGelen(TransferToTigerRequest dto, int userId) => new()
        {
            IslemKodu = dto.ExternalUniqueKey,
            Tarih = dto.TransactionDate,
            FisNo = dto.FisNo ?? dto.RefNo ?? "AUTO",
            Tutar = dto.Amount,
            BankaHesapKodu = dto.BankaHesapKodu ?? "",
            OlusturanKullanici = userId,
            DataReference = dto.DataReference,
            ARP_CODE = dto.ArpCode ?? "",
            GUID = NewGuidUpper(),
            DUE_DATE = dto.TransactionDate
        };

        private static GidenHavaleRequest BuildGiden(TransferToTigerRequest dto, int userId) => new()
        {
            IslemKodu = dto.ExternalUniqueKey,
            Tarih = dto.TransactionDate,
            FisNo = dto.FisNo ?? dto.RefNo ?? "AUTO",
            Tutar = dto.Amount,
            BankaHesapKodu = dto.BankaHesapKodu ?? "",
            OlusturanKullanici = userId,
            DataReference = dto.DataReference,
            ARP_CODE = dto.ArpCode ?? "",
            GUID = NewGuidUpper(),
            DUE_DATE = dto.TransactionDate
        };

        private static VirmanRequest BuildVirman(TransferToTigerRequest dto, int userId) => new()
        {
            IslemKodu = dto.ExternalUniqueKey,
            Tarih = dto.TransactionDate,
            FisNo = dto.FisNo ?? dto.RefNo ?? "AUTO",
            Tutar = dto.Amount,
            BankaHesapKodu = dto.BankaHesapKodu ?? "",
            HedefHesapKodu = dto.HedefHesapKodu ?? "",
            OlusturanKullanici = userId,
            DataReference = dto.DataReference,
            GUID = NewGuidUpper()
        };

        private static KrediTaksitRequest BuildKredi(TransferToTigerRequest dto, int userId) => new()
        {
            IslemKodu = dto.ExternalUniqueKey,
            Tarih = dto.TransactionDate,
            FisNo = dto.FisNo ?? dto.RefNo ?? "AUTO",
            BankaHesapKodu = dto.BankaHesapKodu ?? "",
            KrediHesapKodu = dto.HedefHesapKodu ?? "",
            OlusturanKullanici = userId,
            DataReference = dto.DataReference,
            GUID = NewGuidUpper(),
            ToplamTutar = dto.Amount,
            AnaParaTutar = dto.Amount,
            FaizTutar = 0,
            BsmvTutar = 0,
            VadeTarihi = dto.TransactionDate
        };
    }
}
