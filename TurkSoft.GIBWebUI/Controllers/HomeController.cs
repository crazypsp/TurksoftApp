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

        // JSON cache için statik alanlar
        private static readonly object _mukellefLock = new object();
        private static List<MukellefSlim> _mukellefCache;
        private static DateTime _mukellefFileLastWriteUtc = DateTime.MinValue;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index() => View();

        /// <summary>
        /// JSON satırı için sadeleştirilmiş model
        /// </summary>
        private sealed class MukellefSlim
        {
            public string Identifier { get; set; }
            public string Title { get; set; }
            public string Alias { get; set; }
            public string RegisterType { get; set; }
            public string Type { get; set; }
            public string DocType { get; set; }
            public string FirstRegistrationTime { get; set; }
        }

        /// <summary>
        /// JSON dosyasını okuyup belleğe cache'ler
        /// </summary>
        private List<MukellefSlim> LoadAllMukellef(string filePath)
        {
            lock (_mukellefLock)
            {
                if (!System.IO.File.Exists(filePath))
                {
                    _mukellefCache = new List<MukellefSlim>();
                    _mukellefFileLastWriteUtc = DateTime.MinValue;
                    return _mukellefCache;
                }

                var fi = new FileInfo(filePath);
                var lastWrite = fi.LastWriteTimeUtc;

                // Dosya değişmemişse cache'i kullan
                if (_mukellefCache != null && lastWrite == _mukellefFileLastWriteUtc)
                {
                    return _mukellefCache;
                }

                var json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _mukellefCache = new List<MukellefSlim>();
                    _mukellefFileLastWriteUtc = lastWrite;
                    return _mukellefCache;
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

                var list = new List<MukellefSlim>();

                foreach (var item in items)
                {
                    string identifier = GetStringCaseInsensitive(item, new[] { "Identifier", "identifier", "Vkn", "vkn" }) ?? "";
                    string title = GetStringCaseInsensitive(item, new[] { "Title", "title" }) ?? "";
                    string alias = GetStringCaseInsensitive(item, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";

                    // Ek alanlar
                    string registerType = GetStringCaseInsensitive(item, new[] { "RegisterType", "registerType" }) ?? "";
                    string type = GetStringCaseInsensitive(item, new[] { "Type", "type" }) ?? "";
                    string docType = GetStringCaseInsensitive(item, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                    string firstRegTime =
                        GetStringCaseInsensitive(item, new[] { "FirstRegistrationTime", "firstRegistrationTime",
                                                                "RegisterTime", "registerTime",
                                                                "CreatedAt", "createdAt" }) ?? "";

                    if (string.IsNullOrWhiteSpace(identifier) &&
                        string.IsNullOrWhiteSpace(title) &&
                        string.IsNullOrWhiteSpace(alias))
                    {
                        continue;
                    }

                    list.Add(new MukellefSlim
                    {
                        Identifier = identifier.Trim(),
                        Title = title.Trim(),
                        Alias = alias.Trim(),
                        RegisterType = registerType.Trim(),
                        Type = type.Trim(),
                        DocType = docType.Trim(),
                        FirstRegistrationTime = firstRegTime.Trim()
                    });
                }

                _mukellefCache = list;
                _mukellefFileLastWriteUtc = lastWrite;

                return _mukellefCache;
            }
        }

        /// <summary>
        /// JSON'dan mükellef listesi döner.
        /// 
        /// - Eğer page/pageSize GELMEZ ise: Eski davranış (en fazla 100 kayıt, dizi).
        /// - Eğer page/pageSize GELİR ise: { total, items[] } formatında, tam sayfalama.
        /// </summary>
        [HttpGet]
        public IActionResult SearchMukellef(
            string term,
            int? page,
            int? pageSize,
            string type,
            string docType)
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dataDir = Path.Combine(webRoot, "data");
                var filePath = Path.Combine(dataDir, "gibusers_invoice_receipt_list.json");

                var all = LoadAllMukellef(filePath);

                term ??= string.Empty;
                term = term.Trim();
                var hasTerm = !string.IsNullOrEmpty(term);
                var lowerTerm = term.ToLowerInvariant();

                type = (type ?? string.Empty).Trim().ToUpperInvariant();
                docType = (docType ?? string.Empty).Trim().ToUpperInvariant();

                IEnumerable<MukellefSlim> query = all;

                if (hasTerm)
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.Identifier) &&
                         x.Identifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(x.Title) &&
                         x.Title.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(x =>
                        string.Equals(x.Type ?? string.Empty, type, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(docType))
                {
                    query = query.Where(x =>
                        string.Equals(x.DocType ?? string.Empty, docType, StringComparison.OrdinalIgnoreCase));
                }

                var totalCount = query.Count();

                // Sayfalama isteniyorsa: { total, items[] }
                if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
                {
                    var skip = (page.Value - 1) * pageSize.Value;
                    var pageItems = query
                        .Skip(skip)
                        .Take(pageSize.Value)
                        .Select(x => new
                        {
                            Identifier = x.Identifier,
                            Title = x.Title,
                            Alias = x.Alias,
                            RegisterType = x.RegisterType,
                            Type = x.Type,
                            DocType = x.DocType,
                            FirstRegistrationTime = x.FirstRegistrationTime
                        })
                        .ToList();

                    return Json(new
                    {
                        total = totalCount,
                        items = pageItems
                    });
                }

                // Eski çağrılar (page yok) için: en fazla 100 kayıtlık basit dizi
                var list100 = query
                    .Take(100)
                    .Select(x => new
                    {
                        Identifier = x.Identifier,
                        Title = x.Title,
                        Alias = x.Alias,
                        RegisterType = x.RegisterType,
                        Type = x.Type,
                        DocType = x.DocType,
                        FirstRegistrationTime = x.FirstRegistrationTime
                    })
                    .ToList();

                return Json(list100);
            }
            catch
            {
                if (page.HasValue && pageSize.HasValue)
                {
                    return Json(new
                    {
                        total = 0,
                        items = Array.Empty<object>()
                    });
                }

                return Json(Array.Empty<object>());
            }
        }

        private static string GetStringCaseInsensitive(JsonElement item, string[] propertyNames)
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
