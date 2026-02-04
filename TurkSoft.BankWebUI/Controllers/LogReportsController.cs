using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
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
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Log Raporları";
            ViewData["Subtitle"] = "Tiger Aktarımları + Sistem Logları";

            var transfer = (await _transferLogService.GetAllTransferLogsAsync())?.OrderByDescending(x => x.CreatedDate).Take(1000).ToList() ?? new();
            var system = (await _systemLogService.GetAllSystemLogsAsync())?.OrderByDescending(x => x.CreatedDate).Take(1000).ToList() ?? new();

            ViewBag.TransferLogs = transfer;
            ViewBag.SystemLogs = system;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportTransferJson()
        {
            var transfer = (await _transferLogService.GetAllTransferLogsAsync())?.OrderByDescending(x => x.CreatedDate).ToList() ?? new();
            var json = JsonSerializer.Serialize(transfer, new JsonSerializerOptions { WriteIndented = true });
            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8", $"transferlogs_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSystemJson()
        {
            var system = (await _systemLogService.GetAllSystemLogsAsync())?.OrderByDescending(x => x.CreatedDate).ToList() ?? new();
            var json = JsonSerializer.Serialize(system, new JsonSerializerOptions { WriteIndented = true });
            return File(Encoding.UTF8.GetBytes(json), "application/json; charset=utf-8", $"systemlogs_{DateTime.Now:yyyyMMdd_HHmm}.json");
        }
    }
}
