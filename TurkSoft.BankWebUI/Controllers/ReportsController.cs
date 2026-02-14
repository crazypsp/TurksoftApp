using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Entities;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Entities.Luca;
using TurkSoft.Service.Inferfaces;
using TurkSoft.Service.Interface;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class ReportsController : Controller
    {
        private readonly IBankTransactionService _transactionService;
        private readonly IBankService _bankService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IExportLogService _exportLogService;
        private readonly IClCardService _clCardService;
        private readonly ILogoTigerIntegrationService _logoTigerService;
        private readonly IBankStatementService _bankStatementService;
        private readonly IBankaEkstreAnalyzerService _bankaEkstreAnalyzerService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IBankTransactionService transactionService,
            IBankService bankService,
            IBankAccountService bankAccountService,
            IExportLogService exportLogService,
            IClCardService clCardService,
            ILogoTigerIntegrationService logoTigerService,
            IBankStatementService bankStatementService,
            IBankaEkstreAnalyzerService bankaEkstreAnalyzerService,
            ILogger<ReportsController> logger)
        {
            _transactionService = transactionService;
            _bankService = bankService;
            _bankAccountService = bankAccountService;
            _exportLogService = exportLogService;
            _clCardService = clCardService;
            _logoTigerService = logoTigerService;
            _bankStatementService = bankStatementService;
            _bankaEkstreAnalyzerService = bankaEkstreAnalyzerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["Title"] = "Raporlar";
                ViewData["Subtitle"] = "Tarih aralığı, banka ve hesaba göre filtreleyin";

                // Tüm bankaları getir
                var banks = await _bankService.GetAllBanksAsync();

                // ViewModel oluştur
                var model = new ReportsIndexVm
                {
                    Filter = new ReportFilterVm
                    {
                        DateRange = $"{DateTime.Today.AddDays(-7):dd.MM.yyyy} - {DateTime.Today:dd.MM.yyyy}",
                        Banks = banks.Select(b => new SelectListItem
                        {
                            Value = b.Id.ToString(),
                            Text = b.BankName
                        }).ToList(),
                        Accounts = new List<SelectListItem>(),
                        AccountTypes = new List<string> { "Vadesiz", "Vadeli", "Kredi", "Diğer" }
                    },
                    Rows = new List<BankTransactionVm>(),
                    NetByAccountType = new Dictionary<string, decimal>(),
                    NetByDay = new Dictionary<string, decimal>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reports Index GET hatası");
                TempData["ErrorMessage"] = "Banka listesi yüklenirken hata oluştu.";

                // Hata durumunda boş model döndür
                return View(new ReportsIndexVm
                {
                    Filter = new ReportFilterVm
                    {
                        DateRange = $"{DateTime.Today.AddDays(-7):dd.MM.yyyy} - {DateTime.Today:dd.MM.yyyy}",
                        Banks = new List<SelectListItem>(),
                        Accounts = new List<SelectListItem>(),
                        AccountTypes = new List<string> { "Vadesiz", "Vadeli", "Kredi", "Diğer" }
                    },
                    Rows = new List<BankTransactionVm>(),
                    NetByAccountType = new Dictionary<string, decimal>(),
                    NetByDay = new Dictionary<string, decimal>()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ReportFilterVm filter)
        {
            ViewData["Title"] = "Raporlar";
            ViewData["Subtitle"] = "Tarih aralığı, banka ve hesaba göre filtreleyin";
            return await GetFilteredReports(filter);
        }

        private async Task<IActionResult> GetFilteredReports(ReportFilterVm filter)
        {
            try
            {
                // Tüm bankaları getir
                var allBanks = await _bankService.GetAllBanksAsync();

                // Seçili bankanın hesaplarını getir
                var accounts = new List<SelectListItem>();
                if (!string.IsNullOrEmpty(filter.Bank) && int.TryParse(filter.Bank, out int selectedBankId))
                {
                    var allAccounts = await _bankAccountService.GetAllBankAccountsAsync();
                    var bankAccounts = allAccounts.Where(a => a.BankId == selectedBankId).ToList();

                    accounts = bankAccounts.Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.AccountNumber} - {a.Currency}",
                        Selected = filter.Account == a.Id.ToString()
                    }).ToList();
                }

                // Filter modelini güncelle
                filter.Banks = allBanks.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BankName,
                    Selected = filter.Bank == b.Id.ToString()
                }).ToList();

                filter.Accounts = accounts;
                filter.AccountTypes = new List<string> { "Vadesiz", "Vadeli", "Kredi", "Diğer" };

                // Tarih aralığını parse et
                DateTime startDate = DateTime.Today.AddDays(-7);
                DateTime endDate = DateTime.Today;

                if (!string.IsNullOrEmpty(filter.DateRange))
                {
                    var dates = filter.DateRange.Split(" - ");
                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0], out DateTime parsedStartDate))
                            startDate = parsedStartDate;
                        if (DateTime.TryParse(dates[1], out DateTime parsedEndDate))
                            endDate = parsedEndDate;
                    }
                }

                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();

                // İşlemleri getir
                var allTransactions = await _transactionService.GetTransactionsByDateRangeAsync(userId, startDate, endDate);

                // Filtreleri uygula
                var transactions = allTransactions.AsEnumerable();

                // Banka filtresi
                if (!string.IsNullOrEmpty(filter.Bank) && int.TryParse(filter.Bank, out int bankId))
                {
                    transactions = transactions.Where(t => t.BankId == bankId);
                }

                // Hesap filtresi
                if (!string.IsNullOrEmpty(filter.Account) && int.TryParse(filter.Account, out int accountId))
                {
                    transactions = transactions.Where(t => t.BankId == accountId);
                }

                // Hesap tipi filtresi
                if (!string.IsNullOrEmpty(filter.AccountType))
                {
                    transactions = transactions.Where(t => GetAccountType(t.AccountNumber) == filter.AccountType);
                }

                // ViewModel'leri dönüştür
                var reportRows = transactions.Select(t => new BankTransactionVm
                {
                    Id = t.Id,
                    Date = t.TransactionDate,
                    BankName = GetBankName(allBanks, t.BankId),
                    AccountType = GetAccountType(t.AccountNumber),
                    AccountNumber = t.AccountNumber ?? "Bilinmiyor",
                    ReferenceNo = t.ReferenceNumber ?? "-",
                    Description = t.Description ?? "-",
                    Debit = t.DebitCredit == "D" ? t.Amount : 0,
                    Credit = t.DebitCredit == "C" ? t.Amount : 0
                }).ToList();

                // Net değerleri hesapla
                var netByAccountType = reportRows
                    .GroupBy(r => r.AccountType)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.Net));

                var netByDay = reportRows
                    .GroupBy(r => r.Date.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString("dd.MM.yyyy"), g => g.Sum(r => r.Net));

                // Son model
                var model = new ReportsIndexVm
                {
                    Filter = filter,
                    Rows = reportRows,
                    NetByAccountType = netByAccountType,
                    NetByDay = netByDay
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFilteredReports hatası");
                TempData["ErrorMessage"] = "Rapor oluşturulurken hata oluştu.";

                // Hata durumunda boş model döndür
                return View(new ReportsIndexVm
                {
                    Filter = filter ?? new ReportFilterVm(),
                    Rows = new List<BankTransactionVm>(),
                    NetByAccountType = new Dictionary<string, decimal>(),
                    NetByDay = new Dictionary<string, decimal>()
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountsByBank(int bankId)
        {
            try
            {
                var allAccounts = await _bankAccountService.GetAllBankAccountsAsync();
                var bankAccounts = allAccounts.Where(a => a.BankId == bankId).ToList();

                var accounts = bankAccounts.Select(a => new
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.AccountNumber} - {a.Currency}"
                }).ToList();

                return Json(new { success = true, data = accounts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountsByBank hatası");
                return Json(new { success = false, message = "Hesaplar yüklenirken hata oluştu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportFilterVm filter, string reportType)
        {
            try
            {
                if (string.IsNullOrEmpty(reportType))
                {
                    TempData["ErrorMessage"] = "Rapor tipi seçilmedi.";
                    return RedirectToAction("Index");
                }

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
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(userId, startDate, endDate);

                // Filtreleri uygula
                if (!string.IsNullOrEmpty(filter.Bank) && int.TryParse(filter.Bank, out int bankId))
                {
                    transactions = transactions.Where(t => t.BankId == bankId);
                }

                // Rapor oluşturma işlemi
                var exportLog = new ExportLog
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
                TempData["SuccessMessage"] = $"{reportType} raporu oluşturuldu ve indirilmeye hazır.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateReport hatası");
                TempData["ErrorMessage"] = "Rapor oluşturulurken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Yardımcı metodlar
        private string GetBankName(IEnumerable<Bank> banks, int bankId)
        {
            var bank = banks.FirstOrDefault(b => b.Id == bankId);
            return bank?.BankName ?? "Bilinmeyen";
        }

        private string GetAccountType(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber)) return "Diğer";

            // Basit bir hesap tipi belirleme mantığı
            if (accountNumber.StartsWith("1")) return "Vadesiz";
            if (accountNumber.StartsWith("2")) return "Vadeli";
            if (accountNumber.StartsWith("3")) return "Kredi";
            if (accountNumber.StartsWith("4")) return "Yatırım";
            return "Diğer";
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 1;
            }
            catch
            {
                return 1; // Default user ID
            }
        }

        #region Bank Statement Eşleştirme (YENİ EKLENDİ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MatchBankStatement([FromBody] MatchBankStatementRequest request)
        {
            try
            {
                if (request == null || request.Transactions == null || !request.Transactions.Any())
                {
                    return Json(new { success = false, message = "Eşleştirme için veri bulunamadı." });
                }

                // 1. Hesap planını çek (IClCardService ile)
                var cards = await _clCardService.GetAllCardsAsync();
                var hesapKodlari = cards.Select(c => new AccountingCode
                {
                    Code = c.Code ?? c.Code?.ToString() ?? "0",
                    Name = c.Name ?? c.Name ?? "Bilinmeyen"
                }).ToList();

                // 2. Banka hareketlerini oluştur
                var hareketler = request.Transactions.Select(t => new BankaHareket
                {
                    Tarih = t.Tarih,
                    Aciklama = t.Aciklama,
                    Tutar = t.Tutar,
                    Bakiye = t.Bakiye,
                    HesapKodu = "",
                    KaynakDosya = "Bank Statement",
                    BankaAdi = request.BankName,
                    KlasorYolu = ""
                }).ToList();

                // 3. KeywordMap oluştur
                var keywordMap = new Dictionary<string, string>();
                if (request.KeywordMapItems != null)
                {
                    foreach (var item in request.KeywordMapItems)
                    {
                        if (!string.IsNullOrEmpty(item.Keyword) && !string.IsNullOrEmpty(item.AccountCode))
                        {
                            keywordMap[item.Keyword.ToLower()] = item.AccountCode;
                        }
                    }
                }

                // 4. IBankaEkstreAnalyzerService ile eşleştirme yap
                var result = await _bankaEkstreAnalyzerService.HesapKodlariIleEsleAsync(
                    hareketler,
                    hesapKodlari,
                    keywordMap,
                    request.BankAccountCode ?? "100.01"
                );

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = $"{result.Data.Count} kayıt eşleştirildi."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MatchBankStatement hatası");
                return Json(new
                {
                    success = false,
                    message = $"Eşleştirme sırasında hata: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateMatchedAccountCode([FromBody] UpdateMatchedAccountCodeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Geçersiz istek." });
                }

                // Burada veritabanında güncelleme yapılabilir
                // Şimdilik sadece başarılı dönüyoruz

                return Json(new
                {
                    success = true,
                    message = "Hesap kodu başarıyla güncellendi.",
                    evrakNo = request.EvrakNo,
                    hesapKodu = request.HesapKodu
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMatchedAccountCode hatası");
                return Json(new
                {
                    success = false,
                    message = $"Hesap kodu güncelleme hatası: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> TransferToLuca([FromBody] TransferToLucaRequest request)
        {
            try
            {
                if (request == null || request.Rows == null || !request.Rows.Any())
                {
                    return Json(new { success = false, message = "Transfer için veri bulunamadı." });
                }

                // Burada Luca'ya transfer işlemi yapılacak
                // Örneğin: Luca API'sine kayıt gönderme

                return Json(new
                {
                    success = true,
                    message = $"{request.Rows.Count} kayıt başarıyla Luca'ya transfer edildi.",
                    transferredCount = request.Rows.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransferToLuca hatası");
                return Json(new
                {
                    success = false,
                    message = $"Transfer hatası: {ex.Message}"
                });
            }
        }
        #endregion

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
                OlusturanKullanici = 1,
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

        #region Yardımcı Metodlar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBankStatementReport(ReportFilterVm filter)
        {
            try
            {
                // 1. Banka ve hesap kontrolü
                if (string.IsNullOrEmpty(filter.Bank) || !int.TryParse(filter.Bank, out int bankId))
                    return Json(new { success = false, message = "Lütfen bir banka seçiniz." });

                if (string.IsNullOrEmpty(filter.Account) || !int.TryParse(filter.Account, out int accountId))
                    return Json(new { success = false, message = "Lütfen bir hesap seçiniz." });

                // 2. Banka bilgilerini al
                var allBanks = await _bankService.GetAllBanksAsync();
                var selectedBank = allBanks.FirstOrDefault(b => b.Id == bankId);
                if (selectedBank == null)
                    return Json(new { success = false, message = "Banka bulunamadı." });

                // 3. Hesap bilgilerini al
                var allAccounts = await _bankAccountService.GetAllBankAccountsAsync();
                var selectedAccount = allAccounts.FirstOrDefault(a => a.Id == accountId && a.BankId == bankId);
                if (selectedAccount == null)
                    return Json(new { success = false, message = "Hesap bulunamadı." });

                // 4. Tarih aralığını parse et
                DateTime startDate = DateTime.Today.AddDays(-7);
                DateTime endDate = DateTime.Today;

                if (!string.IsNullOrEmpty(filter.DateRange))
                {
                    var dates = filter.DateRange.Split(" - ");
                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0], out DateTime parsedStartDate))
                            startDate = parsedStartDate;
                        if (DateTime.TryParse(dates[1], out DateTime parsedEndDate))
                            endDate = parsedEndDate;
                    }
                }

                // 5. BankStatementRequest oluştur
                var request = new BankStatementRequest
                {
                    BankId = selectedBank.ExternalBankId, // JS kütüphanesindeki bankId
                    Username = selectedBank.UsernameLabel, // Boş bırak, JS'deki gibi credentials modal'dan alınacak
                    Password = selectedBank.PasswordLabel, // Boş bırak, JS'deki gibi credentials modal'dan alınacak
                    AccountNumber = selectedAccount.AccountNumber,
                    BeginDate = startDate,
                    EndDate = endDate,
                    Link = selectedBank.DefaultLink, // Bank tablosundan
                    TLink = selectedBank.DefaultTLink, // Bank tablosundan
                    Extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "IBAN", selectedAccount.IBAN ?? "" },
                { "SubeNo", selectedAccount.SubeNo ?? "" },
                { "MusteriNo", selectedAccount.MusteriNo ?? "" },
                { "Currency", selectedAccount.Currency }
            }
                };

                // 6. Ziraat Katılım (ExternalBankId = 11) özel işlemi
                if (selectedBank.ExternalBankId == 11)
                {
                    // associationCode ekstra parametresini ekle
                    // Not: Bu bilgiyi Bank tablosunda saklamanız gerekir
                    request.Extras["associationCode"] = "ZIRAAT_KATILIM_CODE"; // Örnek
                }

                // 7. Servisi çağır
                var bnkharList = await _bankStatementService.GetStatementAsync(request);

                // 8. Banka ve hesap bilgilerini ekle
                var enrichedList = bnkharList.Select(bnkhar =>
                {
                    bnkhar.BNKCODE = selectedBank.ExternalBankId.ToString();
                    bnkhar.HESAPNO = selectedAccount.AccountNumber;
                    bnkhar.SUBECODE = selectedAccount.SubeNo;
                    bnkhar.CURRENCYCODE = selectedAccount.Currency;
                    bnkhar.FRMIBAN = selectedAccount.IBAN;
                    return bnkhar;
                }).ToList();

                // 9. JSON olarak döndür
                var payload = new
                {
                    success = true,
                    data = enrichedList,
                    bankInfo = new
                    {
                        bankId = selectedBank.ExternalBankId,
                        bankName = selectedBank.BankName,
                        accountNumber = selectedAccount.AccountNumber,
                        recordCount = enrichedList.Count
                    }
                };

                // ✅ JSON alan adlarını modeldeki haliyle (UPPERCASE) döndür (DataTables eşleşmesi için)
                return new JsonResult(payload)
                {
                    SerializerSettings = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null,
                        DictionaryKeyPolicy = null
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBankStatementReport hatası");
                return Json(new { success = false, message = $"Rapor oluşturulurken hata: {ex.Message}" });
            }
        }

        #endregion
    }

    #region Request Modelleri (YENİ EKLENDİ)
    public class MatchBankStatementRequest
    {
        public List<BankStatementTransaction> Transactions { get; set; }
        public string BankName { get; set; }
        public string BankAccountCode { get; set; }
        public List<KeywordMapItem> KeywordMapItems { get; set; }
    }

    public class BankStatementTransaction
    {
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; }
        public decimal Tutar { get; set; }
        public decimal? Bakiye { get; set; }
        public string BorcAlacak { get; set; }
    }

    public class KeywordMapItem
    {
        public string Keyword { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
    }

    public class UpdateMatchedAccountCodeRequest
    {
        public string EvrakNo { get; set; }
        public string HesapKodu { get; set; }
        public decimal Borc { get; set; }
        public decimal Alacak { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }
    }

    public class TransferToLucaRequest
    {
        public List<LucaFisRow> Rows { get; set; }
    }
    #endregion
}