using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class ReportsController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly IExportLogService _exportLogService;
        private readonly IClCardService _clCardService;
        private readonly ILogoTigerIntegrationService _logoTigerService;
        private readonly ILogger<ReportsController> _logger;
        public ReportsController(
            IBankTransactionService transactionService,
            IBankService bankService,
            IExportLogService exportLogService, 
            IClCardService clCardService,
            ILogoTigerIntegrationService logoTigerService,
            ILogger<ReportsController> logger)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _exportLogService = exportLogService;
            _clCardService = clCardService;
            _logoTigerService = logoTigerService;
            _logger = logger;
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
        #region Gelen Havale İşlemleri
        [HttpGet]
        public IActionResult GelenHavale()
        {
            var model = new GelenHavaleRequest
            {
                IslemKodu = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                Tarih = DateTime.Today,
                DUE_DATE = DateTime.Today.AddDays(7),
                Saat = DateTime.Now.Hour,
                Dakika = DateTime.Now.Minute,
                Saniye = DateTime.Now.Second,
                TYPE = 3,
                TRCODE = 3,
                MODULENR = 7,
                BANK_PROC_TYPE = 2,
                OlusturanKullanici = 1, // Örnek kullanıcı ID
                GUID = Guid.NewGuid().ToString().ToUpper()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GelenHavale(GelenHavaleRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _logoTigerService.GelenHavaleEkleAsync(request);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Data.Mesaj;
                        TempData["FisReferans"] = result.Data.FisReferans;
                        TempData["IslemKodu"] = result.Data.IslemKodu;
                        TempData["GUID"] = result.Data.GUID;

                        return RedirectToAction(nameof(IslemSonuc), new
                        {
                            fisRef = result.Data.FisReferans,
                            islemKodu = result.Data.IslemKodu
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        ViewBag.ErrorMessage = result.Message;
                    }
                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelen havale işleminde hata oluştu.");
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View(request);
            }
        }
        #endregion

        #region Giden Havale İşlemleri
        [HttpGet]
        public IActionResult GidenHavale()
        {
            var model = new GidenHavaleRequest
            {
                IslemKodu = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                Tarih = DateTime.Today,
                DUE_DATE = DateTime.Today.AddDays(7),
                Saat = DateTime.Now.Hour,
                Dakika = DateTime.Now.Minute,
                Saniye = DateTime.Now.Second,
                TYPE = 4,
                TRCODE = 4,
                MODULENR = 7,
                BANK_PROC_TYPE = 2,
                SIGN = 1,
                OlusturanKullanici = 1,
                GUID = Guid.NewGuid().ToString().ToUpper()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GidenHavale(GidenHavaleRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _logoTigerService.GidenHavaleEkleAsync(request);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Data.Mesaj;
                        TempData["FisReferans"] = result.Data.FisReferans;
                        TempData["IslemKodu"] = result.Data.IslemKodu;
                        TempData["GUID"] = result.Data.GUID;

                        return RedirectToAction(nameof(IslemSonuc), new
                        {
                            fisRef = result.Data.FisReferans,
                            islemKodu = result.Data.IslemKodu
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        ViewBag.ErrorMessage = result.Message;
                    }
                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giden havale işleminde hata oluştu.");
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View(request);
            }
        }
        #endregion

        #region Virman İşlemleri
        [HttpGet]
        public IActionResult Virman()
        {
            var model = new VirmanRequest
            {
                IslemKodu = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                Tarih = DateTime.Today,
                Saat = DateTime.Now.Hour,
                Dakika = DateTime.Now.Minute,
                Saniye = DateTime.Now.Second,
                TYPE = 2,
                TRCODE = 2,
                MODULENR = 7,
                OlusturanKullanici = 1,
                GUID = Guid.NewGuid().ToString().ToUpper()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Virman(VirmanRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _logoTigerService.VirmanEkleAsync(request);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Data.Mesaj;
                        TempData["FisReferans"] = result.Data.FisReferans;
                        TempData["IslemKodu"] = result.Data.IslemKodu;
                        TempData["GUID"] = result.Data.GUID;

                        return RedirectToAction(nameof(IslemSonuc), new
                        {
                            fisRef = result.Data.FisReferans,
                            islemKodu = result.Data.IslemKodu
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        ViewBag.ErrorMessage = result.Message;
                    }
                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Virman işleminde hata oluştu.");
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View(request);
            }
        }
        #endregion

        #region Kredi Taksit İşlemleri
        [HttpGet]
        public IActionResult KrediTaksit()
        {
            var model = new KrediTaksitRequest
            {
                IslemKodu = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper(),
                Tarih = DateTime.Today,
                VadeTarihi = DateTime.Today.AddMonths(1),
                Saat = DateTime.Now.Hour,
                Dakika = DateTime.Now.Minute,
                Saniye = DateTime.Now.Second,
                TYPE = 1,
                TRCODE = 1,
                MODULENR = 7,
                OlusturanKullanici = 1,
                GUID = Guid.NewGuid().ToString().ToUpper(),
                TaksitNo = 1,
                AnaParaTutar = 1000,
                FaizTutar = 100,
                BsmvTutar = 50,
                ToplamTutar = 1150
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KrediTaksit(KrediTaksitRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _logoTigerService.KrediTaksitOdemeEkleAsync(request);

                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Data.Mesaj;
                        TempData["FisReferans"] = result.Data.FisReferans;
                        TempData["IslemKodu"] = result.Data.IslemKodu;
                        TempData["GUID"] = result.Data.GUID;
                        TempData["OdenenAnaPara"] = result.Data.OdenenAnaPara;
                        TempData["OdenenFaiz"] = result.Data.OdenenFaiz;
                        TempData["OdenenBsmv"] = result.Data.OdenenBsmv;
                        TempData["ToplamOdenen"] = result.Data.ToplamOdenen;
                        TempData["TaksitNo"] = result.Data.TaksitNo;

                        return RedirectToAction(nameof(KrediTaksitSonuc), new
                        {
                            fisRef = result.Data.FisReferans,
                            islemKodu = result.Data.IslemKodu
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        ViewBag.ErrorMessage = result.Message;
                    }
                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kredi taksit işleminde hata oluştu.");
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View(request);
            }
        }
        #endregion

        #region İşlem Durumu Kontrol
        [HttpGet]
        public IActionResult IslemDurumu()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IslemDurumu(int fisReferans)
        {
            try
            {
                if (fisReferans <= 0)
                {
                    ModelState.AddModelError("fisReferans", "Geçerli bir fiş referansı giriniz.");
                    return View();
                }

                var result = await _logoTigerService.IslemDurumuKontrolAsync(fisReferans);

                if (result.Success)
                {
                    ViewBag.DurumSonuc = result.Data;
                    ViewBag.FisReferans = fisReferans;
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    ViewBag.ErrorMessage = result.Message;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İşlem durumu kontrolünde hata oluştu.");
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View();
            }
        }
        #endregion

        #region Sonuç Sayfaları
        [HttpGet]
        public IActionResult IslemSonuc(int fisRef, string islemKodu)
        {
            ViewBag.FisReferans = fisRef;
            ViewBag.IslemKodu = islemKodu;
            ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
            ViewBag.GUID = TempData["GUID"]?.ToString();

            return View();
        }

        [HttpGet]
        public IActionResult KrediTaksitSonuc(int fisRef, string islemKodu)
        {
            ViewBag.FisReferans = fisRef;
            ViewBag.IslemKodu = islemKodu;
            ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
            ViewBag.GUID = TempData["GUID"]?.ToString();
            ViewBag.OdenenAnaPara = TempData["OdenenAnaPara"]?.ToString();
            ViewBag.OdenenFaiz = TempData["OdenenFaiz"]?.ToString();
            ViewBag.OdenenBsmv = TempData["OdenenBsmv"]?.ToString();
            ViewBag.ToplamOdenen = TempData["ToplamOdenen"]?.ToString();
            ViewBag.TaksitNo = TempData["TaksitNo"]?.ToString();

            return View();
        }
        #endregion

        #region AJAX Metodları
        [HttpPost]
        public async Task<JsonResult> AjaxGelenHavale([FromBody] GelenHavaleRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Geçersiz veri." });
                }

                var result = await _logoTigerService.GelenHavaleEkleAsync(request);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Data,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX gelen havale işleminde hata oluştu.");
                return Json(new { success = false, message = $"İşlem sırasında bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AjaxIslemDurumu(int fisReferans)
        {
            try
            {
                if (fisReferans <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz fiş referansı." });
                }

                var result = await _logoTigerService.IslemDurumuKontrolAsync(fisReferans);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Data,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX işlem durumu kontrolünde hata oluştu.");
                return Json(new { success = false, message = $"İşlem sırasında bir hata oluştu: {ex.Message}" });
            }
        }
        #endregion
    }
}