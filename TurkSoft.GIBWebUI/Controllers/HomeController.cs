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
            public string RegisterType { get; set; }
            public string Type { get; set; }
            public string DocType { get; set; }
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

                type = (type ?? string.Empty).Trim().ToUpperInvariant();
                docType = (docType ?? string.Empty).Trim().ToUpperInvariant();

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
                        string registerType = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";
                        string typeVal = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";
                        string docTypeVal = GetStringCaseInsensitive(element, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                        string firstRegTime = GetStringCaseInsensitive(element, new[]
                        {
                            "FirstRegistrationTime", "firstRegistrationTime",
                            "RegisterTime",          "registerTime",
                            "FirstCreationTime",     "firstCreationTime",
                            "CreatedAt",             "createdAt"
                        }) ?? "";

                        // ===== Filtreler =====

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

                        if (!string.IsNullOrEmpty(type))
                        {
                            if (!string.Equals(typeVal ?? string.Empty, type, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        if (!string.IsNullOrEmpty(docType))
                        {
                            if (!string.Equals(docTypeVal ?? string.Empty, docType, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // Bu noktaya geldiyse kayıt filtreye uyuyor
                        if (hasPaging)
                        {
                            // filteredCount: 0-based index
                            if (filteredCount >= skip && filteredCount < skip + take)
                            {
                                resultItems.Add(new MukellefSlim
                                {
                                    Identifier = identifier,
                                    Title = title,
                                    Alias = alias,
                                    RegisterType = registerType,
                                    Type = typeVal,
                                    DocType = docTypeVal,
                                    FirstRegistrationTime = firstRegTime
                                });
                            }

                            filteredCount++;
                        }
                        else
                        {
                            // Sayfalama yoksa en fazla 100 kayıt döndürelim
                            resultItems.Add(new MukellefSlim
                            {
                                Identifier = identifier,
                                Title = title,
                                Alias = alias,
                                RegisterType = registerType,
                                Type = typeVal,
                                DocType = docTypeVal,
                                FirstRegistrationTime = firstRegTime
                            });

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

                        // Eşleşme bulundu → diğer alanları da al
                        string title = GetStringCaseInsensitive(element, new[] { "Title", "title" }) ?? "";
                        string alias = GetStringCaseInsensitive(element, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";
                        string registerType = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";
                        string typeVal = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";
                        string docTypeVal = GetStringCaseInsensitive(element, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                        string firstRegTime = GetStringCaseInsensitive(element, new[]
                        {
                            "FirstRegistrationTime", "firstRegistrationTime",
                            "RegisterTime",          "registerTime",
                            "FirstCreationTime",     "firstCreationTime",
                            "CreatedAt",             "createdAt"
                        }) ?? "";

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
                            FirstRegistrationTime = firstRegTime
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
