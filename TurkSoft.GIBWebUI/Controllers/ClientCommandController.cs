using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ClientCommandController : Controller
    {
        // Bağlı tüm clientların durumlarını tutar
        private static readonly ConcurrentDictionary<string, DateTime> _heartbeats
            = new ConcurrentDictionary<string, DateTime>();

        // Her client'ın komut kuyruğu
        private static readonly ConcurrentDictionary<string, string> _commands
            = new ConcurrentDictionary<string, string>();

        // ================================
        // 1) Agent → Heartbeat
        // ================================
        [HttpPost]
        public IActionResult Heartbeat(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required");

            _heartbeats[clientId] = DateTime.Now;
            return Json(new { status = "OK" });
        }

        // ================================
        // 2) Web → Sunucuya komut yollar
        // ================================
        [HttpPost]
        public IActionResult Send([FromBody] JsonElement json, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required");

            _commands[clientId] = json.ToString();
            return Json(new { status = "COMMAND_RECEIVED" });
        }

        // ================================
        // 3) Agent → Komut Çekme
        // ================================
        [HttpGet]
        public IActionResult Get(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("clientId is required");

            if (_commands.TryGetValue(clientId, out string cmd))
            {
                return Content(cmd);
            }

            return Content("NONE");
        }

        // ================================
        // 4) Agent → Komutu temizleme
        // ================================
        [HttpGet]
        public IActionResult Clear(string clientId)
        {
            _commands.TryRemove(clientId, out _);
            return Json(new { status = "CLEARED" });
        }

        // ================================
        // 5) Web → Agent online kontrol
        // ================================
        [HttpGet]
        public IActionResult IsOnline(string clientId)
        {
            if (_heartbeats.TryGetValue(clientId, out DateTime t))
            {
                bool online = (DateTime.Now - t).TotalSeconds < 10;
                return Json(new { online });
            }

            return Json(new { online = false });
        }
    }
}
