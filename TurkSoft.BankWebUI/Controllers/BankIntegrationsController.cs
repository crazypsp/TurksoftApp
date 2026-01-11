using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TurkSoft.BankWebUI.Models;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize(Roles = "Admin,Integrator,Finance")]
    public sealed class BankIntegrationsController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IBankCredentialService _credentialService;
        private readonly ITransactionImportService _importService;

        public BankIntegrationsController(
            IBankService bankService,
            IBankCredentialService credentialService,
            ITransactionImportService importService)
        {
            _bankService = bankService;
            _credentialService = credentialService;
            _importService = importService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Banka Entegrasyonları";
            ViewData["Subtitle"] = "Bağlantı ayarları, çekim planı, son çekim ve durum";

            var banks = await _bankService.GetAllBanksAsync();
            var credentials = await _credentialService.GetAllCredentialsAsync();
            var imports = await _importService.GetAllImportsAsync();

            var model = new BankIntegrationsVm
            {
                Items = banks.Select(b => new BankIntegrationSetting
                {
                    Id = b.Id.ToString(),
                    BankName = b.BankName,
                    ConnectionType = ConnectionType.Api, // Varsayılan değer
                    IsActive = b.IsActive,
                    PullSchedule = "Her gün 06:00",
                    LastPullAt = imports
                        .Where(i => i.BankId == b.Id && i.Status == "SUCCESS")
                        .OrderByDescending(i => i.CompletedAt)
                        .FirstOrDefault()?.CompletedAt,
                    NextPullAt = DateTime.UtcNow.AddHours(6),
                    LastStatus = GetLastStatus(imports, b.Id),
                    LastMessage = GetLastMessage(imports, b.Id),
                    CredentialsConfigured = credentials.Any(c => c.BankId == b.Id)
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Logs(string? bank)
        {
            ViewData["Title"] = "Çekim Logları";
            ViewData["Subtitle"] = "Bankadan çekim denemeleri ve sonuçları";

            var imports = await _importService.GetAllImportsAsync();
            var banks = await _bankService.GetAllBanksAsync();

            var filteredImports = imports.AsEnumerable();
            if (!string.IsNullOrEmpty(bank) && int.TryParse(bank, out int bankId))
            {
                filteredImports = filteredImports.Where(i => i.BankId == bankId);
            }

            var model = new BankPullLogsVm
            {
                Bank = bank,
                Banks = banks.Select(b => b.BankName).ToList(),
                Logs = filteredImports
                    .OrderByDescending(i => i.StartedAt)
                    .Select(i => new BankPullLog
                    {
                        Id = i.Id,
                        SettingId = i.BankId.ToString(),
                        BankName = banks.FirstOrDefault(b => b.Id == i.BankId)?.BankName ?? "Bilinmeyen",
                        FromAt = i.StartDate,
                        ToAt = i.EndDate,                        
                        Message = i.ErrorMessage ?? "Başarılı",
                        NewTransactionCount = i.ImportedRecords
                    })
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Delta(int logId)
        {
            var import = await _importService.GetImportByIdAsync(logId);
            if (import == null) return NotFound();

            var bank = await _bankService.GetBankByIdAsync(import.BankId);

            var model = new BankPullDeltaVm
            {
                LogId = logId,
                BankName = bank?.BankName ?? "Bilinmeyen",
                FromAt = import.StartDate,
                ToAt = import.EndDate,
                Transactions = new List<Models.BankTransaction>() // Bu kısım transaction service ile doldurulabilir
            };

            ViewData["Title"] = "Son Çekimden Beri Gelenler";
            ViewData["Subtitle"] = $"{model.BankName} · {model.FromAt:dd.MM.yyyy HH:mm} → {model.ToAt:dd.MM.yyyy HH:mm}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string id)
        {
            if (int.TryParse(id, out int bankId))
            {
                var bank = await _bankService.GetBankByIdAsync(bankId);
                if (bank == null) return NotFound();

                bank.IsActive = !bank.IsActive;
                bank.ModifiedDate = DateTime.UtcNow;

                await _bankService.UpdateBankAsync(bank);
                TempData["Toast"] = "Entegrasyon durumu güncellendi.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PullNow(string id)
        {
            if (int.TryParse(id, out int bankId))
            {
                var bank = await _bankService.GetBankByIdAsync(bankId);
                if (bank == null) return NotFound();

                var import = new TransactionImport
                {
                    UserId = GetCurrentUserId(),
                    BankId = bankId,
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow,
                    TotalRecords = new Random().Next(40, 60),
                    ImportedRecords = new Random().Next(35, 55),
                    Status = "SUCCESS",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow.AddMinutes(2),
                    RequestParameters = "{\"days\": 7}"
                };

                await _importService.CreateImportAsync(import);
                TempData["Toast"] = $"{bank.BankName} için çekim başlatıldı.";
            }

            return RedirectToAction("Index");
        }

        private BankPullStatus GetLastStatus(IEnumerable<TransactionImport> imports, int bankId)
        {
            var lastImport = imports
                .Where(i => i.BankId == bankId)
                .OrderByDescending(i => i.CompletedAt)
                .FirstOrDefault();

            if (lastImport == null) return BankPullStatus.Ok;

            return lastImport.Status == "SUCCESS" ? BankPullStatus.Ok :
                   lastImport.Status == "PARTIAL" ? BankPullStatus.Warning :
                   BankPullStatus.Error;
        }

        private string GetLastMessage(IEnumerable<TransactionImport> imports, int bankId)
        {
            var lastImport = imports
                .Where(i => i.BankId == bankId)
                .OrderByDescending(i => i.CompletedAt)
                .FirstOrDefault();

            return lastImport?.ErrorMessage ?? "Son çekim başarılı";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
        }
    }
}