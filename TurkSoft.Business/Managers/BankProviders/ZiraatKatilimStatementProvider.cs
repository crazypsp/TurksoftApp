using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class ZiraatKatilimStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.ZiraatKatilim;
        public string BankCode => "ZKT";

        private readonly IHttpClientFactory _httpClientFactory;

        public ZiraatKatilimStatementProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            var associationCode = request.GetExtraRequired("associationCode");

            // SoapUI: Endpoint = https://zkapigw.ziraatkatilim.com.tr:8443  Resource = /api/accountService
            // Kullanıcı Link'i ister host olarak, ister full path olarak verebilir.
            var (baseAddress, resourcePath) = ResolveEndpoint(request.Link);

            // Basic Auth:
            // - Extras["authorization"] varsa kullan (Basic xxx veya sadece xxx)
            // - Yoksa Username:Password'tan üret
            var (scheme, parameter) = ResolveAuthorization(request);

            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = baseAddress;
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);

            var result = new List<BNKHAR>();

            // Banka max 7 gün istiyor -> otomatik parçala
            foreach (var (from, to) in SplitMax7Days(request.BeginDate, request.EndDate))
            {
                var body = new
                {
                    associationCode,
                    // SoapUI ekranındaki gibi: 2025-12-29T00:00:00 (Z yok, ms yok)
                    startDate = from.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                    endDate = to.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                };

                using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                using var resp = await http.PostAsync(resourcePath, content, ct).ConfigureAwait(false);

                var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"ZiraatKatılım API hata {(int)resp.StatusCode}: {json}");

                ParseAccountServiceJson(json, request.AccountNumber, BankCode, result);
            }

            return result;
        }

        private static (Uri baseAddress, string resourcePath) ResolveEndpoint(string? link)
        {
            // Default (dokümandaki / SoapUI'daki gibi)
            var defaultBase = new Uri("https://zkapigw.ziraatkatilim.com.tr:8443");
            const string defaultPath = "/api/accountService";

            if (string.IsNullOrWhiteSpace(link))
                return (defaultBase, defaultPath);

            // link hem host hem full path gelebilir
            var uri = new Uri(link.Trim(), UriKind.Absolute);

            var basePart = new Uri(uri.GetLeftPart(UriPartial.Authority));
            var path = uri.AbsolutePath;

            if (string.IsNullOrWhiteSpace(path) || path == "/")
                path = defaultPath;

            // yanlışlıkla sonuna / eklenmiş olabilir
            if (!path.StartsWith("/")) path = "/" + path;

            return (basePart, path);
        }

        private static (string scheme, string parameter) ResolveAuthorization(BankStatementRequest request)
        {
            var authRaw = request.GetExtra("authorization");

            if (!string.IsNullOrWhiteSpace(authRaw))
            {
                authRaw = authRaw.Trim();

                // "Basic abcdef..." şeklinde geldiyse ayır
                if (authRaw.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    return ("Basic", authRaw.Substring("Basic ".Length).Trim());

                // Sadece base64 geldiyse
                return ("Basic", authRaw);
            }

            // Username:Password'tan üret
            var raw = $"{request.Username}:{request.Password}";
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
            return ("Basic", b64);
        }

        private static IEnumerable<(DateTime from, DateTime to)> SplitMax7Days(DateTime begin, DateTime end)
        {
            if (end < begin) throw new ArgumentException("EndDate BeginDate'den küçük olamaz.");

            var cur = begin;
            while (cur <= end)
            {
                // 7 gün aralık: cur .. cur+7gün-1sn
                var chunkEnd = cur.AddDays(7).AddSeconds(-1);
                if (chunkEnd > end) chunkEnd = end;

                yield return (cur, chunkEnd);

                cur = chunkEnd.AddSeconds(1);
            }
        }

        private static void ParseAccountServiceJson(string json, string? filterAccountNo, string bankCode, List<BNKHAR> target)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!TryGet(root, "AccountReportV2Response", out var resp)) return;
            if (!TryGet(resp, "AccountReportV2Result", out var res)) return;
            if (!TryGet(res, "AccountDetail", out var detail)) return;
            if (!TryGet(detail, "BankaHesaplariClassDetail", out var bhcd)) return;
            if (!TryGet(bhcd, "BankaHesaplari", out var bankaHesaplari)) return;

            // ÖNEMLİ: HesapBilgisiDetail burada array
            if (!TryGet(bankaHesaplari, "HesapBilgisiDetail", out var hesapBilgisiDetail)) return;

            foreach (var hbd in EnumerateObjectOrArray(hesapBilgisiDetail))
            {
                if (!TryGet(hbd, "HesapTanimi", out var ht)) continue;

                var hesapNo = GetString(ht, "HesapNumarasi") ?? "";
                if (!string.IsNullOrWhiteSpace(filterAccountNo) &&
                    !string.Equals(filterAccountNo.Trim(), hesapNo, StringComparison.OrdinalIgnoreCase))
                    continue;

                var musteriNo = GetString(ht, "MusteriNo") ?? "";
                var subeNo = GetString(ht, "SubeNumarasi") ?? "";
                var currency = GetString(ht, "HesapCinsi");
                var iban = GetString(ht, "Iban");

                // Bazı cevaplarda HesapHareketleri "" (string) gelebilir => hareket yok demektir.
                if (!TryGet(hbd, "HesapHareketleri", out var hh)) continue;
                if (hh.ValueKind == JsonValueKind.String) continue;
                if (hh.ValueKind != JsonValueKind.Object && hh.ValueKind != JsonValueKind.Array) continue;

                if (!TryGet(hh, "HesapHareketiDetail", out var hareketDetay)) continue;

                foreach (var hareket in EnumerateObjectOrArray(hareketDetay))
                {
                    var tarih = GetString(hareket, "Tarih");
                    var saat = GetString(hareket, "Saat");
                    var dt = ParseTrDateTime(tarih, saat);

                    var dekontNo = GetString(hareket, "DekontNo");
                    var referansNo = GetString(hareket, "ReferansNo");
                    var tutar = GetString(hareket, "HareketTutari") ?? "0";
                    var bakiye = GetString(hareket, "SonBakiye") ?? "0";

                    target.Add(new BNKHAR
                    {
                        BNKCODE = bankCode,
                        HESAPNO = hesapNo,
                        URF = musteriNo,
                        SUBECODE = subeNo,
                        CURRENCYCODE = currency,

                        PROCESSIBAN = iban,
                        PROCESSID = !string.IsNullOrWhiteSpace(referansNo) ? referansNo : (dekontNo ?? ""),
                        PROCESSREFNO = dekontNo ?? "",

                        PROCESSTIMESTR = $"{tarih} {saat}".Trim(),
                        PROCESSTIMESTR2 = tarih ?? "",
                        PROCESSTIME = dt,
                        PROCESSTIME2 = dt,

                        PROCESSAMAOUNT = tutar,
                        PROCESSBALANCE = bakiye,
                        PROCESSDESC = GetString(hareket, "Aciklamalar") ?? GetString(hareket, "Aciklama") ?? "",
                        PROCESSVKN = GetString(hareket, "KarsiHesapVKNo") ?? "",
                        PROCESSTYPECODE = GetString(hareket, "IslemKodu") ?? "",
                        PROCESSDEBORCRED = IsDebit(tutar) ? "B" : "A",

                        Durum = 0
                    });
                }
            }
        }

        private static bool IsDebit(string amount)
            => amount.TrimStart().StartsWith("-", StringComparison.Ordinal);

        private static DateTime? ParseTrDateTime(string? ddMMyyyy, string? hhmmss)
        {
            var s = $"{ddMMyyyy} {hhmmss}".Trim();
            if (string.IsNullOrWhiteSpace(s)) return null;

            var tr = CultureInfo.GetCultureInfo("tr-TR");
            return DateTime.TryParse(s, tr, DateTimeStyles.AssumeLocal, out var dt) ? dt : null;
        }

        private static IEnumerable<JsonElement> EnumerateObjectOrArray(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Array => element.EnumerateArray(),
                JsonValueKind.Object => new[] { element },
                _ => Array.Empty<JsonElement>()
            };
        }

        private static bool TryGet(JsonElement obj, string name, out JsonElement value)
        {
            if (obj.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in obj.EnumerateObject())
                {
                    if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = p.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        private static string? GetString(JsonElement obj, string name)
        {
            if (!TryGet(obj, name, out var v)) return null;
            return v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
        }
    }
}
