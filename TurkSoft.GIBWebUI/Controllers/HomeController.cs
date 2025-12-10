using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        /// JSON satırı için sadeleştirilmiş model
        /// (sunucuya döndürdüğümüz tip)
        /// </summary>
        private sealed class MukellefSlim
        {
            public string Identifier { get; set; }
            public string Title { get; set; }
            public string Alias { get; set; }
            public string RegisterType { get; set; }        // Özel / Kamu
            public string Type { get; set; }                // PK / GB
            public string DocType { get; set; }             // e-Fatura / e-İrsaliye
            public string FirstRegistrationTime { get; set; }
        }

        /// <summary>
        /// wwwroot\data\gibusers_invoice_receipt_list.json yolunu üretir
        /// </summary>
        private string GetJsonFilePath()
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dataDir = Path.Combine(webRoot, "data");
            var filePath = Path.Combine(dataDir, "gibusers_invoice_receipt_list.json");

            Console.WriteLine($"[HomeController] webRoot = {webRoot}");
            Console.WriteLine($"[HomeController] JSON path = {filePath}");

            return filePath;
        }

        /// <summary>
        /// JsonElement içinden, verilen property adlarından ilk bulduğunu string olarak döndürür.
        /// (Case-sensitive; bu yüzden farklı varyantları dizi olarak veriyoruz.)
        /// </summary>
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

        private static string MapGibUserType(string s)
        {
            return s switch
            {
                "1" => "Özel",
                "2" => "Kamu",
                _ => string.Empty
            };
        }

        private static string MapGibAliasType(string s)
        {
            return s switch
            {
                "1" => "PK",
                "2" => "GB",
                _ => string.Empty
            };
        }

        private static string MapAppType(string s)
        {
            return s switch
            {
                "1" => "e-Fatura",
                "3" => "e-İrsaliye",
                _ => string.Empty
            };
        }

        private static string NormalizeDateTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            if (DateTime.TryParse(raw, out var dt))
                return dt.ToString("yyyy-MM-dd HH:mm:ss");

            return raw.Replace('T', ' ');
        }

        // ======================================================
        //  SEARCH MÜKELLEF  (streaming JSON)
        // ======================================================

        [HttpGet]
        public async Task<IActionResult> SearchMukellef(
            string term,
            int? page,
            int? pageSize,
            string type,
            string docType)
        {
            try
            {
                var filePath = GetJsonFilePath();

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[SearchMukellef] Dosya bulunamadı: {filePath}");
                    if (page.HasValue && pageSize.HasValue)
                    {
                        return Json(new { total = 0, items = Array.Empty<object>() });
                    }
                    return Json(Array.Empty<object>());
                }

                term ??= string.Empty;
                term = term.Trim();
                bool hasTerm = term.Length > 0;

                // Tip = GibAliasType (1: PK, 2: GB)
                type = (type ?? string.Empty).Trim();
                bool hasTypeFilter = !string.IsNullOrEmpty(type);

                // Belge Tip = AppType (1: e-Fatura, 3: e-İrsaliye)
                docType = (docType ?? string.Empty).Trim();
                bool hasDocTypeFilter = !string.IsNullOrEmpty(docType);

                bool hasPaging = page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0;
                int skip = hasPaging ? (page.Value - 1) * pageSize.Value : 0;
                int take = hasPaging ? pageSize.Value : 0;

                var resultItems = new List<MukellefSlim>();
                int filteredCount = 0; // filtreye uyan toplam kayıt sayısı

                await using (var fs = new FileStream(
                                 filePath,
                                 FileMode.Open,
                                 FileAccess.Read,
                                 FileShare.ReadWrite,
                                 bufferSize: 1024 * 64,
                                 useAsync: true))
                {
                    await foreach (var element in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(fs))
                    {
                        if (element.ValueKind != JsonValueKind.Object)
                            continue;

                        string identifier = GetStringCaseInsensitive(element, new[] { "Identifier", "identifier", "Vkn", "vkn" }) ?? "";
                        string title = GetStringCaseInsensitive(element, new[] { "Title", "title" }) ?? "";
                        string alias = GetStringCaseInsensitive(element, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";

                        // Yeni JSON alanları
                        string gibUserType = GetStringCaseInsensitive(element, new[] { "GibUserType", "gibUserType" }) ?? "";
                        string gibAliasType = GetStringCaseInsensitive(element, new[] { "GibAliasType", "gibAliasType" }) ?? "";
                        string appType = GetStringCaseInsensitive(element, new[] { "AppType", "appType" }) ?? "";

                        string firstRegTime = GetStringCaseInsensitive(element, new[]
                        {
                            "FirstRegistrationTime", "firstRegistrationTime",
                            "RegisterTime",          "registerTime",
                            "FirstCreationTime",     "firstCreationTime",
                            "CreatedAt",             "createdAt"
                        }) ?? "";

                        // ===== term filtresi =====
                        if (hasTerm)
                        {
                            bool matchTerm =
                                (!string.IsNullOrEmpty(identifier) &&
                                 identifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                                ||
                                (!string.IsNullOrEmpty(title) &&
                                 title.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);

                            if (!matchTerm)
                                continue;
                        }

                        // ===== Tip filtresi (GibAliasType: 1 PK, 2 GB) =====
                        if (hasTypeFilter)
                        {
                            if (!string.Equals(gibAliasType, type, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // ===== Belge Tip filtresi (AppType: 1 e-Fatura, 3 e-İrsaliye) =====
                        if (hasDocTypeFilter)
                        {
                            if (!string.Equals(appType, docType, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // ===== Görünen metinler =====
                        string registerType = MapGibUserType(gibUserType);
                        if (string.IsNullOrEmpty(registerType))
                        {
                            // Eski JSON'da zaten yazılı ise kullan
                            registerType = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";
                        }

                        string typeVal = MapGibAliasType(gibAliasType);
                        if (string.IsNullOrEmpty(typeVal))
                        {
                            typeVal = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";
                        }

                        string docTypeVal = MapAppType(appType);
                        if (string.IsNullOrEmpty(docTypeVal))
                        {
                            docTypeVal = GetStringCaseInsensitive(element, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                            if (string.Equals(docTypeVal, "INVOICE", StringComparison.OrdinalIgnoreCase))
                                docTypeVal = "e-Fatura";
                            else if (string.Equals(docTypeVal, "DESPATCHADVICE", StringComparison.OrdinalIgnoreCase))
                                docTypeVal = "e-İrsaliye";
                        }

                        var slim = new MukellefSlim
                        {
                            Identifier = identifier,
                            Title = title,
                            Alias = alias,
                            RegisterType = registerType,
                            Type = typeVal,
                            DocType = docTypeVal,
                            FirstRegistrationTime = NormalizeDateTime(firstRegTime)
                        };

                        if (hasPaging)
                        {
                            if (filteredCount >= skip && filteredCount < skip + take)
                            {
                                resultItems.Add(slim);
                            }

                            filteredCount++;
                        }
                        else
                        {
                            resultItems.Add(slim);
                            filteredCount++;
                            if (resultItems.Count >= 100)
                                break;
                        }
                    }
                }

                Console.WriteLine($"[SearchMukellef] filteredCount = {filteredCount}, returned = {resultItems.Count}");

                if (hasPaging)
                {
                    return Json(new
                    {
                        total = filteredCount,
                        items = resultItems
                    });
                }

                return Json(resultItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SearchMukellef] HATA: " + ex);

                if (page.HasValue && pageSize.HasValue)
                {
                    return Json(new { total = 0, items = Array.Empty<object>() });
                }

                return Json(Array.Empty<object>());
            }
        }

        // ======================================================
        //  GetMukellefByIdentifier  (streaming JSON)
        // ======================================================

        [HttpGet]
        public async Task<IActionResult> GetMukellefByIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return Json(new { found = false });
            }

            // Sadece rakamları al, 10–11 haneye indir
            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            if (digits.Length < 10 || digits.Length > 11)
            {
                return Json(new { found = false });
            }

            try
            {
                var filePath = GetJsonFilePath();

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[GetMukellefByIdentifier] Dosya bulunamadı: {filePath}");
                    return Json(new { found = false });
                }

                await using (var fs = new FileStream(
                                 filePath,
                                 FileMode.Open,
                                 FileAccess.Read,
                                 FileShare.ReadWrite,
                                 bufferSize: 1024 * 64,
                                 useAsync: true))
                {
                    await foreach (var element in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(fs))
                    {
                        if (element.ValueKind != JsonValueKind.Object)
                            continue;

                        string idVal = GetStringCaseInsensitive(element, new[] { "Identifier", "identifier", "Vkn", "vkn" }) ?? "";
                        if (string.IsNullOrEmpty(idVal))
                            continue;

                        var xDigits = new string(idVal.Where(char.IsDigit).ToArray());
                        if (!string.Equals(xDigits, digits, StringComparison.Ordinal))
                            continue;

                        // eşleşen kaydı slim'e map edelim
                        string title = GetStringCaseInsensitive(element, new[] { "Title", "title" }) ?? "";
                        string alias = GetStringCaseInsensitive(element, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";

                        string gibUserType = GetStringCaseInsensitive(element, new[] { "GibUserType", "gibUserType" }) ?? "";
                        string gibAliasType = GetStringCaseInsensitive(element, new[] { "GibAliasType", "gibAliasType" }) ?? "";
                        string appType = GetStringCaseInsensitive(element, new[] { "AppType", "appType" }) ?? "";

                        string firstRegTime = GetStringCaseInsensitive(element, new[]
                        {
                            "FirstRegistrationTime", "firstRegistrationTime",
                            "RegisterTime",          "registerTime",
                            "FirstCreationTime",     "firstCreationTime",
                            "CreatedAt",             "createdAt"
                        }) ?? "";

                        string registerType = MapGibUserType(gibUserType)
                                              ?? GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";

                        string typeVal = MapGibAliasType(gibAliasType)
                                         ?? GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";

                        string docTypeVal = MapAppType(appType);
                        if (string.IsNullOrEmpty(docTypeVal))
                        {
                            docTypeVal = GetStringCaseInsensitive(element, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                            if (string.Equals(docTypeVal, "INVOICE", StringComparison.OrdinalIgnoreCase))
                                docTypeVal = "e-Fatura";
                            else if (string.Equals(docTypeVal, "DESPATCHADVICE", StringComparison.OrdinalIgnoreCase))
                                docTypeVal = "e-İrsaliye";
                        }

                        Console.WriteLine($"[GetMukellefByIdentifier] FOUND match for {digits}: {title}, {alias}");

                        return Json(new
                        {
                            found = true,
                            Identifier = idVal,
                            Title = title,
                            Alias = alias,
                            RegisterType = registerType,
                            Type = typeVal,
                            DocType = docTypeVal,
                            FirstRegistrationTime = NormalizeDateTime(firstRegTime)
                        });
                    }
                }

                Console.WriteLine($"[GetMukellefByIdentifier] NO MATCH for {digits}");
                return Json(new { found = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GetMukellefByIdentifier] ERROR: " + ex);
                return Json(new { found = false });
            }
        }
    }
}
