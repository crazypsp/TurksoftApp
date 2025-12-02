using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index() => View();

        /// <summary>
        /// wwwroot/data/gibusers_invoice_receipt_list.json dosyasını okur,
        /// Identifier / Title'a göre sunucu tarafında filtreler.
        /// </summary>
        [HttpGet]
        public IActionResult SearchMukellef(string term)
        {
            try
            {
                var webRoot = _env.WebRootPath;
                var dataDir = Path.Combine(webRoot, "data");
                var filePath = Path.Combine(dataDir, "gibusers_invoice_receipt_list.json");

                if (!System.IO.File.Exists(filePath))
                {
                    // Dosya yoksa boş liste dön
                    return Json(Array.Empty<object>());
                }

                var json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return Json(Array.Empty<object>());
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                IEnumerable<JsonElement> items;

                if (root.ValueKind == JsonValueKind.Array)
                    items = root.EnumerateArray();
                else if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                    items = itemsProp.EnumerateArray();
                else if (root.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
                    items = resultsProp.EnumerateArray();
                else if (root.TryGetProperty("list", out var listProp) && listProp.ValueKind == JsonValueKind.Array)
                    items = listProp.EnumerateArray();
                else
                    items = new[] { root };

                term ??= string.Empty;
                term = term.Trim();
                var hasTerm = !string.IsNullOrEmpty(term);
                var lowerTerm = term.ToLowerInvariant();

                var result = new List<object>();

                foreach (var item in items)
                {
                    string identifier = GetStringCaseInsensitive(item, new[] { "Identifier", "identifier", "Vkn", "vkn" }) ?? "";
                    string title = GetStringCaseInsensitive(item, new[] { "Title", "title" }) ?? "";
                    string alias = GetStringCaseInsensitive(item, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";

                    if (string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(title) && string.IsNullOrEmpty(alias))
                        continue;

                    if (hasTerm)
                    {
                        var match =
                            identifier.ToLowerInvariant().Contains(lowerTerm) ||
                            title.ToLowerInvariant().Contains(lowerTerm);

                        if (!match)
                            continue;
                    }

                    var id = GetStringCaseInsensitive(item, new[] { "Id", "id" }) ?? identifier;

                    result.Add(new
                    {
                        id,
                        Identifier = identifier.Trim(),
                        Title = title.Trim(),
                        Alias = alias.Trim()
                    });

                    // Çok büyük listelerde performans için 100 kayıtla sınırla
                    if (result.Count >= 100)
                        break;
                }

                return Json(result);
            }
            catch
            {
                return Json(Array.Empty<object>());
            }
        }

        private static string? GetStringCaseInsensitive(JsonElement item, string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                if (item.TryGetProperty(name, out var val))
                {
                    if (val.ValueKind == JsonValueKind.String)
                        return val.GetString();

                    if (val.ValueKind == JsonValueKind.Number && val.TryGetInt64(out var n))
                        return n.ToString();
                }
            }
            return null;
        }
    }
}
