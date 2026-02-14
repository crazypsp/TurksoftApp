using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TurkSoft.BankWebUI.ViewModels;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.BankWebUI.Controllers
{
    [Authorize]
    public sealed class LogReportsController : Controller
    {
        private readonly ITransferLogService _transferLogService;
        private readonly ISystemLogService _systemLogService;

        public LogReportsController(ITransferLogService transferLogService, ISystemLogService systemLogService)
        {
            _transferLogService = transferLogService;
            _systemLogService = systemLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] LogReportFilterVm filter)
        {
            ViewData["Title"] = "Log Raporları";
            ViewData["Subtitle"] = "Tiger Aktarımları + Sistem Logları (tarih filtreli)";

            var vm = new LogReportsIndexVm
            {
                IsAdmin = User.IsInRole("Admin"),
                CurrentUserId = GetUserId(),
                Filter = filter ?? new LogReportFilterVm()
            };

            // Default tarih: son 7 gün
            var from = vm.Filter.From?.Date ?? DateTime.Today.AddDays(-7);
            var to = vm.Filter.To?.Date ?? DateTime.Today;
            var toEnd = to.AddDays(1).AddTicks(-1);

            // Admin değilse sadece kendi logunu görür
            if (!vm.IsAdmin)
                vm.Filter.UserId = vm.CurrentUserId;

            // Transfer logs
            var transfer = (await _transferLogService.GetAllTransferLogsAsync())?.ToList() ?? new List<TransferLog>();
            transfer = transfer
                .Where(x => x.CreatedDate >= from && x.CreatedDate <= toEnd)
                .ToList();

            if (vm.Filter.UserId.HasValue)
                transfer = transfer.Where(x => x.UserId == vm.Filter.UserId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(vm.Filter.Status))
                transfer = transfer.Where(x => string.Equals(x.Status, vm.Filter.Status, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(vm.Filter.TargetSystem))
                transfer = transfer.Where(x => string.Equals(x.TargetSystem, vm.Filter.TargetSystem, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(vm.Filter.Query))
            {
                var q = vm.Filter.Query.Trim();
                transfer = transfer.Where(x =>
                        (x.ExternalUniqueKey ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (x.ErrorMessage ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (x.RequestData ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (x.ResponseData ?? "").Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            vm.TransferLogs = transfer
                .OrderByDescending(x => x.CreatedDate)
                .Take(2000)
                .ToList();

            // System logs
            var system = (await _systemLogService.GetAllSystemLogsAsync())?.ToList() ?? new List<SystemLog>();
            system = system
                .Where(x => x.CreatedDate >= from && x.CreatedDate <= toEnd)
                .ToList();

            if (vm.Filter.UserId.HasValue)
                system = system.Where(x => x.UserId == vm.Filter.UserId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(vm.Filter.LogLevel))
                system = system.Where(x => string.Equals(x.LogLevel, vm.Filter.LogLevel, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(vm.Filter.Query))
            {
                var q = vm.Filter.Query.Trim();
                system = system.Where(x =>
                        (x.Message ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (x.Source ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (x.ActionName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            vm.SystemLogs = system
                .OrderByDescending(x => x.CreatedDate)
                .Take(2000)
                .ToList();

            // Filtreyi geri basmak için
            vm.Filter.From = from;
            vm.Filter.To = to;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTransferJson([FromQuery] LogReportFilterVm filter)
        {
            // Export: Index ile aynı filtre mantığını uygula
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserId();

            var from = filter?.From?.Date ?? DateTime.Today.AddDays(-7);
            var to = filter?.To?.Date ?? DateTime.Today;
            var toEnd = to.AddDays(1).AddTicks(-1);

            if (!isAdmin)
                filter.UserId = userId;

            var transfer = (await _transferLogService.GetAllTransferLogsAsync())?.ToList() ?? new List<TransferLog>();
            transfer = transfer.Where(x => x.CreatedDate >= from && x.CreatedDate <= toEnd).ToList();

            if (filter?.UserId.HasValue == true)
                transfer = transfer.Where(x => x.UserId == filter.UserId.Value).ToList();

            var json = JsonSerializer.Serialize(transfer.OrderByDescending(x => x.CreatedDate),
                new JsonSerializerOptions { WriteIndented = true });

            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8",
                $"transferlogs_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSystemJson([FromQuery] LogReportFilterVm filter)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserId();

            var from = filter?.From?.Date ?? DateTime.Today.AddDays(-7);
            var to = filter?.To?.Date ?? DateTime.Today;
            var toEnd = to.AddDays(1).AddTicks(-1);

            if (!isAdmin)
                filter.UserId = userId;

            var system = (await _systemLogService.GetAllSystemLogsAsync())?.ToList() ?? new List<SystemLog>();
            system = system.Where(x => x.CreatedDate >= from && x.CreatedDate <= toEnd).ToList();

            if (filter?.UserId.HasValue == true)
                system = system.Where(x => x.UserId == filter.UserId.Value).ToList();

            var json = JsonSerializer.Serialize(system.OrderByDescending(x => x.CreatedDate),
                new JsonSerializerOptions { WriteIndented = true });

            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8",
                $"systemlogs_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
