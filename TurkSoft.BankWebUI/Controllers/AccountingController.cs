using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Entities.Entities;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;    // ILogoTigerIntegrationService
using TurkSoft.Service.Interface;     // IBankStatementService
using TurkSoft.Services.Interfaces;   // IBankService, IBankAccountService, ITransferLogService, ISystemLogService
using System.Linq;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class AccountingController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankStatementService _bankStatementService;
        private readonly IClCardService _clCardService;
        private readonly ITigerBankAccountService _tigerBankAccountService;
        private readonly ITigerGlAccountService _tigerGlAccountService;
        private readonly ILogoTigerIntegrationService _logoTiger;
        private readonly ITransferLogService _transferLogService;
        private readonly ISystemLogService _systemLogService;

        public AccountingController(
            IBankService bankService,
            IBankAccountService bankAccountService,
            IBankStatementService bankStatementService,
            IClCardService clCardService,
            ITigerBankAccountService tigerBankAccountService,
            ITigerGlAccountService tigerGlAccountService,
            ILogoTigerIntegrationService logoTiger,
            ITransferLogService transferLogService,
            ISystemLogService systemLogService)
        {
            _bankService = bankService;
            _bankAccountService = bankAccountService;
            _bankStatementService = bankStatementService;
            _clCardService = clCardService;
            _tigerBankAccountService = tigerBankAccountService;
            _tigerGlAccountService = tigerGlAccountService;
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
                    var log = logs.FirstOrDefault(l => string.Equals(l.ExternalUniqueKey, key, StringComparison.OrdinalIgnoreCase) && l.Status == "SUCCESS");

                    dto.Add(new BankStatementRowDto
                    {
                        ProcessTimeStr = !string.IsNullOrWhiteSpace(x.PROCESSTIMESTR) ? x.PROCESSTIMESTR : x.PROCESSTIMESTR2,
                        ProcessTime = x.PROCESSTIME,
                        RefNo = x.PROCESSREFNO,
                        Description = string.Join(" ",
                            new[] { x.PROCESSDESC, x.PROCESSDESC2, x.PROCESSDESC3, x.PROCESSDESC4 }
                            .Where(s => !string.IsNullOrWhiteSpace(s)))
                            .Trim(),
                        AmountStr = x.PROCESSAMAOUNT,
                        DebitCredit = NormalizeDebitCredit(x.PROCESSDEBORCRED), // ✅ HATA BURADAYDI -> helper eklendi
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

        // Logo Tiger: Cari / Banka Hesap / Diğer Hesap Planı arama (typeahead)
        [HttpGet]
        public async Task<IActionResult> SearchTigerCaris([FromQuery] string term, [FromQuery] int take = 30)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
                return Json(new { success = true, items = Array.Empty<object>() });

            var items = await _clCardService.SearchCardsAsync(term);
            var result = items
                .Take(take <= 0 ? 30 : take)
                .Select(x => new { code = x.Code, name = x.Name })
                .ToList();

            return Json(new { success = true, items = result });
        }

        [HttpGet]
        public async Task<IActionResult> SearchTigerBankAccounts([FromQuery] string term, [FromQuery] int take = 30)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
                return Json(new { success = true, items = Array.Empty<object>() });

            var items = await _tigerBankAccountService.SearchAsync(term, take <= 0 ? 30 : take);
            var result = items.Select(x => new { code = x.Code, name = x.Name }).ToList();
            return Json(new { success = true, items = result });
        }

        [HttpGet]
        public async Task<IActionResult> SearchTigerOtherAccounts([FromQuery] string term, [FromQuery] int take = 30)
        {
            term = (term ?? "").Trim();
            if (term.Length < 2)
                return Json(new { success = true, items = Array.Empty<object>() });

            var items = await _tigerGlAccountService.SearchAsync(term, take <= 0 ? 30 : take);
            var result = items.Select(x => new { code = x.Code, name = x.Name }).ToList();
            return Json(new { success = true, items = result });
        }

        // Transfer: sonucu normalize ediyoruz (Kredi/BankaFis dönüş tipi farkını kaldırıyoruz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferToTiger([FromBody] TransferToTigerRequest dto)
        {
            var userId = GetUserId();

            var logs = (await _transferLogService.GetAllTransferLogsAsync())?.ToList() ?? new();
            if (logs.Any(l => string.Equals(l.ExternalUniqueKey, dto.ExternalUniqueKey, StringComparison.OrdinalIgnoreCase) && l.Status == "SUCCESS"))
                return Json(new { success = true, alreadyTransferred = true, message = "Bu kayıt daha önce aktarılmış." });

            // ✅ Minimum alan validasyonu (Madde 3)
            if (string.IsNullOrWhiteSpace(dto.BankaHesapKodu))
                return Json(new { success = false, message = "Tiger Banka Hesap Kodu zorunludur." });

            if (IsKredi(dto.Description, dto.TypeCode))
            {
                if (string.IsNullOrWhiteSpace(dto.HedefHesapKodu))
                    return Json(new { success = false, message = "Kredi/Diğer Hesap Kodu (EMUHACC) zorunludur." });
            }
            else if (IsVirman(dto.Description, dto.TypeCode))
            {
                if (string.IsNullOrWhiteSpace(dto.HedefHesapKodu))
                    return Json(new { success = false, message = "Virman için hedef banka hesabı zorunludur." });

                if (string.Equals(dto.BankaHesapKodu?.Trim(), dto.HedefHesapKodu?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "Virman hedef hesabı, kaynak hesap ile aynı olamaz." });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.ArpCode))
                    return Json(new { success = false, message = "Gelen/Giden havale için Cari Kodu (ARP_CODE) zorunludur." });
            }

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
                else if (string.Equals(NormalizeDebitCredit(dto.DebitCredit), "C", StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ dto.DebitCredit null veya A/B gelirse normalize et
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
            public string? DebitCredit { get; set; } // D/C (veya A/B gelebilir)
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
            // default: son 7 gün
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today;

            if (string.IsNullOrWhiteSpace(dateRange))
                return (start, end);

            // flatpickr bazen "dd.MM.yyyy - dd.MM.yyyy" bazen "dd.MM.yyyy to dd.MM.yyyy" dönebiliyor.
            // Bu regex iki tarihi yakalar.
            var m = Regex.Match(dateRange.Trim(), @"(?<d1>\d{1,2}[./-]\d{1,2}[./-]\d{2,4}).*?(?<d2>\d{1,2}[./-]\d{1,2}[./-]\d{2,4})");
            if (!m.Success)
                return (start, end);

            var s1 = m.Groups["d1"].Value;
            var s2 = m.Groups["d2"].Value;

            if (TryParseDate(s1, out var s)) start = s.Date;
            if (TryParseDate(s2, out var e)) end = e.Date;

            if (end < start)
                (start, end) = (end, start);

            return (start, end);
        }

        private static bool TryParseDate(string s, out DateTime dt)
        {
            var tr = new CultureInfo("tr-TR");

            // en sık gelen formatlar
            var formats = new[]
            {
                "dd.MM.yyyy",
                "d.M.yyyy",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy"
            };

            if (DateTime.TryParseExact(s, formats, tr, DateTimeStyles.AssumeLocal, out dt))
                return true;

            if (DateTime.TryParse(s, tr, DateTimeStyles.AssumeLocal, out dt))
                return true;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
                return true;

            dt = default;
            return false;
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
            => (desc ?? "").ToUpperInvariant().Contains("VIRMAN") || (typeCode ?? "").ToUpperInvariant().Contains("VIR");

        private static bool IsKredi(string? desc, string? typeCode)
            => (desc ?? "").ToUpperInvariant().Contains("KREDI")
               || (desc ?? "").ToUpperInvariant().Contains("TAKSIT")
               || (typeCode ?? "").ToUpperInvariant().Contains("KRD");

        /// <summary>
        /// WS bazı bankalarda A/B, bazı bankalarda C/D döndürebiliyor.
        /// UI ve Tiger entegrasyonu için D/C standardına normalize ediyoruz:
        ///  A (Alacak) -> C
        ///  B (Borç)   -> D
        ///  C/D zaten varsa aynen döner.
        ///  null/boş -> null
        /// </summary>
        private static string? NormalizeDebitCredit(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var v = value.Trim().ToUpperInvariant();

            return v switch
            {
                "A" => "C",
                "B" => "D",
                "C" => "C",
                "D" => "D",
                _ => v
            };
        }

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
