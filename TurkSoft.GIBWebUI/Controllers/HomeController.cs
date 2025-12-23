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

        private sealed class MukellefSlim
        {
            public string Identifier { get; set; }
            public string Title { get; set; }
            public string Alias { get; set; }

            // EK: Detay çağrısında tüm aliaslar
            public string[] Aliases { get; set; }

            public string RegisterType { get; set; }
            public string Type { get; set; }
            public string DocType { get; set; }
            public string FirstRegistrationTime { get; set; }
        }

        private string GetJsonFilePath()
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dataDir = Path.Combine(webRoot, "data");
            var filePath = Path.Combine(dataDir, "gibusers_invoice_receipt_list.json");

            Console.WriteLine($"[HomeController] webRoot = {webRoot}");
            Console.WriteLine($"[HomeController] JSON path = {filePath}");

            return filePath;
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

        private static string MapGibUserType(string s) => s switch
        {
            "1" => "Özel",
            "2" => "Kamu",
            _ => string.Empty
        };

        private static string MapGibAliasType(string s) => s switch
        {
            "1" => "PK",
            "2" => "GB",
            _ => string.Empty
        };

        private static string MapAppType(string s) => s switch
        {
            "1" => "e-Fatura",
            "3" => "e-İrsaliye",
            _ => string.Empty
        };

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
                        return Json(new { total = 0, items = Array.Empty<object>() });

                    return Json(Array.Empty<object>());
                }

                term ??= string.Empty;
                term = term.Trim();
                bool hasTerm = term.Length > 0;

                // EK: term sadece rakamsa (VKN/TCKN araması) dedupe yapacağız (madde 1)
                bool termIsAllDigits = hasTerm && term.All(char.IsDigit);

                type = (type ?? string.Empty).Trim();
                bool hasTypeFilter = !string.IsNullOrEmpty(type);

                docType = (docType ?? string.Empty).Trim();
                bool hasDocTypeFilter = !string.IsNullOrEmpty(docType);

                bool hasPaging = page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0;
                int skip = hasPaging ? (page.Value - 1) * pageSize.Value : 0;
                int take = hasPaging ? pageSize.Value : 0;

                var resultItems = new List<MukellefSlim>();

                // toplam sayım: paging varsa Select2 için total dönecek
                int universeCount = 0;

                // EK: VKN/TCKN aramasında aynı Identifier tek kez gelsin
                HashSet<string> seenIdentifiers = termIsAllDigits
                    ? new HashSet<string>(StringComparer.Ordinal)
                    : null;

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
                            if (termIsAllDigits)
                            {
                                // sadece identifier üzerinden ara (madde 1)
                                if (string.IsNullOrEmpty(identifier) || identifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                                    continue;
                            }
                            else
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
                        }

                        // ===== Tip filtresi =====
                        if (hasTypeFilter)
                        {
                            if (!string.Equals(gibAliasType, type, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // ===== Belge Tip filtresi =====
                        if (hasDocTypeFilter)
                        {
                            if (!string.Equals(appType, docType, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // EK: Numeric term ise aynı Identifier'ı 1 kez say/döndür (madde 1)
                        if (termIsAllDigits)
                        {
                            var onlyDigits = new string((identifier ?? "").Where(char.IsDigit).ToArray());
                            if (string.IsNullOrEmpty(onlyDigits))
                                continue;

                            if (!seenIdentifiers.Add(onlyDigits))
                                continue;
                        }

                        string registerType = MapGibUserType(gibUserType);
                        if (string.IsNullOrEmpty(registerType))
                            registerType = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";

                        string typeVal = MapGibAliasType(gibAliasType);
                        if (string.IsNullOrEmpty(typeVal))
                            typeVal = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";

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
                            if (universeCount >= skip && universeCount < skip + take)
                                resultItems.Add(slim);

                            universeCount++;
                        }
                        else
                        {
                            resultItems.Add(slim);
                            universeCount++;
                            if (resultItems.Count >= 100)
                                break;
                        }
                    }
                }

                Console.WriteLine($"[SearchMukellef] universeCount = {universeCount}, returned = {resultItems.Count}");

                if (hasPaging)
                {
                    return Json(new
                    {
                        total = universeCount,
                        items = resultItems
                    });
                }

                return Json(resultItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SearchMukellef] HATA: " + ex);

                if (page.HasValue && pageSize.HasValue)
                    return Json(new { total = 0, items = Array.Empty<object>() });

                return Json(Array.Empty<object>());
            }
        }

        // ======================================================
        //  GetMukellefByIdentifier  (streaming JSON)
        // ======================================================

        [HttpGet]
        public async Task<IActionResult> GetMukellefByIdentifier(string identifier, bool includeAliases = false)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return Json(new { found = false });

            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            if (digits.Length < 10 || digits.Length > 11)
                return Json(new { found = false });

            try
            {
                var filePath = GetJsonFilePath();

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[GetMukellefByIdentifier] Dosya bulunamadı: {filePath}");
                    return Json(new { found = false });
                }

                // includeAliases=false => ilk eşleşmede dön (eski performans)
                if (!includeAliases)
                {
                    await using var fsFast = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        bufferSize: 1024 * 64,
                        useAsync: true);

                    await foreach (var element in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(fsFast))
                    {
                        if (element.ValueKind != JsonValueKind.Object)
                            continue;

                        string idVal = GetStringCaseInsensitive(element, new[] { "Identifier", "identifier", "Vkn", "vkn" }) ?? "";
                        if (string.IsNullOrEmpty(idVal))
                            continue;

                        var xDigits = new string(idVal.Where(char.IsDigit).ToArray());
                        if (!string.Equals(xDigits, digits, StringComparison.Ordinal))
                            continue;

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

                        // düzeltme: Map boşsa fallback
                        string registerType = MapGibUserType(gibUserType);
                        if (string.IsNullOrEmpty(registerType))
                            registerType = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";

                        string typeVal = MapGibAliasType(gibAliasType);
                        if (string.IsNullOrEmpty(typeVal))
                            typeVal = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";

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
                            Aliases = string.IsNullOrWhiteSpace(alias) ? Array.Empty<string>() : new[] { alias },
                            RegisterType = registerType,
                            Type = typeVal,
                            DocType = docTypeVal,
                            FirstRegistrationTime = NormalizeDateTime(firstRegTime)
                        });
                    }

                    Console.WriteLine($"[GetMukellefByIdentifier] NO MATCH for {digits}");
                    return Json(new { found = false });
                }

                // includeAliases=true => aynı Identifier için tüm aliasları topla (madde 2)
                var aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                string foundIdentifier = null;
                string foundTitle = null;

                string registerTypeAgg = null;
                string typeValAgg = null;
                string docTypeValAgg = null;
                string firstRegAgg = null;

                await using var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    bufferSize: 1024 * 64,
                    useAsync: true);

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

                    // eşleşme var
                    foundIdentifier ??= idVal;

                    var title = GetStringCaseInsensitive(element, new[] { "Title", "title" }) ?? "";
                    if (string.IsNullOrWhiteSpace(foundTitle) && !string.IsNullOrWhiteSpace(title))
                        foundTitle = title;

                    var alias = GetStringCaseInsensitive(element, new[] { "Alias", "alias", "GibAlias", "gibAlias" }) ?? "";
                    if (!string.IsNullOrWhiteSpace(alias))
                        aliasSet.Add(alias.Trim());

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

                    // ilk dolu değerleri tut
                    if (string.IsNullOrWhiteSpace(firstRegAgg) && !string.IsNullOrWhiteSpace(firstRegTime))
                        firstRegAgg = firstRegTime;

                    var reg = MapGibUserType(gibUserType);
                    if (string.IsNullOrEmpty(reg))
                        reg = GetStringCaseInsensitive(element, new[] { "RegisterType", "registerType" }) ?? "";
                    if (string.IsNullOrWhiteSpace(registerTypeAgg) && !string.IsNullOrWhiteSpace(reg))
                        registerTypeAgg = reg;

                    var typ = MapGibAliasType(gibAliasType);
                    if (string.IsNullOrEmpty(typ))
                        typ = GetStringCaseInsensitive(element, new[] { "Type", "type" }) ?? "";
                    if (string.IsNullOrWhiteSpace(typeValAgg) && !string.IsNullOrWhiteSpace(typ))
                        typeValAgg = typ;

                    var doc = MapAppType(appType);
                    if (string.IsNullOrEmpty(doc))
                    {
                        doc = GetStringCaseInsensitive(element, new[] { "DocType", "docType", "DocumentType", "documentType" }) ?? "";
                        if (string.Equals(doc, "INVOICE", StringComparison.OrdinalIgnoreCase))
                            doc = "e-Fatura";
                        else if (string.Equals(doc, "DESPATCHADVICE", StringComparison.OrdinalIgnoreCase))
                            doc = "e-İrsaliye";
                    }
                    if (string.IsNullOrWhiteSpace(docTypeValAgg) && !string.IsNullOrWhiteSpace(doc))
                        docTypeValAgg = doc;
                }

                if (foundIdentifier == null)
                {
                    Console.WriteLine($"[GetMukellefByIdentifier] NO MATCH for {digits} (includeAliases=true)");
                    return Json(new { found = false });
                }

                var aliasesArr = aliasSet.ToArray();
                var firstAlias = aliasesArr.FirstOrDefault() ?? "";

                return Json(new
                {
                    found = true,
                    Identifier = foundIdentifier,
                    Title = foundTitle ?? "",
                    Alias = firstAlias,              // geriye uyumluluk
                    Aliases = aliasesArr,            // madde 2
                    RegisterType = registerTypeAgg ?? "",
                    Type = typeValAgg ?? "",
                    DocType = docTypeValAgg ?? "",
                    FirstRegistrationTime = NormalizeDateTime(firstRegAgg ?? "")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GetMukellefByIdentifier] ERROR: " + ex);
                return Json(new { found = false });
            }
        }
    }
}
