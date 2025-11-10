using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using TurkSoft.Entities.GIBEntityDB;
using static TurkSoft.Service.Interface.IGibBusiness;

namespace TurkSoft.Service.Manager
{
    // ------- Uygulama Sınıfı --------
    public sealed class GibBusiness : IGibBusiness
    {
        // ---- Ayarlar (dışarıdan verilebilir) ----
        public sealed class Options
        {
            public string AuthUrl { get; init; } = "https://coretest.isim360.com/v1/token";
            public string Username { get; init; } = "noxyazilim@ui.com";
            public string Password { get; init; } = "asd123";
            public string ClientId { get; init; } = "serviceApi";

            public string EFaturaBaseUrl { get; init; } = "https://efaturaservicetest.isim360.com";
            public string EIrsaliyeBaseUrl { get; init; } = "https://eirsaliyeservicetest.isim360.com";
            public string EDefterBaseUrl { get; init; } = "https://edefterservicetest.isim360.com";

            public string TestSenderVkn { get; init; } = "1234567803";
            public string TestInboxAlias { get; init; } = "urn:mail:defaulttest3pk@medyasoft.com.tr";
        }

        // ---- İç DTO'lar (özel) ----
        private sealed class TokenResponse
        {
            [JsonPropertyName("accessToken")] public string? AccessToken { get; set; }
            [JsonPropertyName("expiresIn")] public int ExpiresIn { get; set; }
        }

        private sealed class OutboxInvoiceCreateRequest
        {
            public int RecordType { get; set; } = 1; // 1:e-Fatura, 0:e-Arşiv
            public int Status { get; set; } = 20;    // Kuyruk & GİB
            public string? LocalReferenceId { get; set; }
            public int? Type { get; set; }           // 2:İhracat
            public AddressBook? AddressBook { get; set; }
            public InvoiceHeader Header { get; set; } = new();
            public List<InvoiceLine> Lines { get; set; } = new();
            public InvoiceTotals Totals { get; set; } = new();
        }
        private sealed class AddressBook { public string? Name { get; set; } public string? IdentificationNumber { get; set; } public string? Alias { get; set; } }
        private sealed class InvoiceHeader { public DateTime IssueDate { get; set; } = DateTime.UtcNow; public string Currency { get; set; } = "TRY"; public string? Note { get; set; } }
        private sealed class InvoiceLine { public string Name { get; set; } = default!; public decimal Quantity { get; set; } public string UnitCode { get; set; } = "C62"; public decimal UnitPrice { get; set; } public decimal VatRate { get; set; } = 20; }
        private sealed class InvoiceTotals { public decimal LineExtensionAmount { get; set; } public decimal TaxExclusiveAmount { get; set; } public decimal TaxInclusiveAmount { get; set; } public decimal PayableAmount { get; set; } }

        private sealed class OutboxDespatchUblRequest
        {
            public string DespatchAdviceZip { get; set; } = default!; // base64 ZIP
            public string? LocalReferenceId { get; set; }
            public string? Prefix { get; set; }
            public string? TargetAlias { get; set; }
            public bool? UseManualDespatchAdviceId { get; set; }
            public bool? CheckLocalReferenceId { get; set; }
            public int Status { get; set; } = 20;
        }

        // ---- Alanlar ----
        private readonly Options _opt;
        private readonly HttpClient _http;
        private string? _token;
        private DateTimeOffset _tokenExp;
        private bool _disposed;

        private sealed class Balance { public int Remaining; public int Used; }
        private readonly ConcurrentDictionary<string, Balance> _balances = new();

        public GibBusiness(Options? options = null, HttpMessageHandler? handler = null)
        {
            _opt = options ?? new Options();
            _http = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
            _http.Timeout = TimeSpan.FromSeconds(100);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _http.Dispose();
            _disposed = true;
        }

        // ---------- Auth ----------
        private async Task EnsureTokenAsync(CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(_token) && _tokenExp > DateTimeOffset.UtcNow.AddSeconds(30))
                return;

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = _opt.Username,
                ["password"] = _opt.Password,
                ["client_id"] = _opt.ClientId
            });
            using var req = new HttpRequestMessage(HttpMethod.Post, _opt.AuthUrl) { Content = form };
            using var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Auth failed: {(int)resp.StatusCode} {await resp.Content.ReadAsStringAsync(ct)}");

            var t = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (string.IsNullOrWhiteSpace(t?.AccessToken))
                throw new InvalidOperationException("Auth failed: empty token");

            _token = t.AccessToken;
            _tokenExp = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, t.ExpiresIn - 15));
        }

        private async Task<HttpRequestMessage> CreateReqAsync(string baseUrl, HttpMethod method, string path, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);
            var req = new HttpRequestMessage(method, new Uri(new Uri(baseUrl), path));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return req;
        }

        private static async Task<HttpResult<T>> SendAsync<T>(HttpClient http, HttpRequestMessage req, CancellationToken ct)
        {
            using var resp = await http.SendAsync(req, ct);
            var code = (int)resp.StatusCode;
            if (!resp.IsSuccessStatusCode)
                return HttpResult<T>.Fail(await resp.Content.ReadAsStringAsync(ct), code);

            if (typeof(T) == typeof(byte[]))
            {
                var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
                return HttpResult<T>.Success((T)(object)bytes, code);
            }
            var data = await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            return HttpResult<T>.Success(data!, code);
        }

        // ---------- Kontör ----------
        Task<(int remaining, int used)> IGibBusiness.GetBalanceAsync(string companyVkn, CancellationToken ct)
        {
            var bal = _balances.GetOrAdd(companyVkn, _ => new Balance { Remaining = 1000, Used = 0 });
            return Task.FromResult((bal.Remaining, bal.Used));
        }

        Task<(int remaining, int used)> IGibBusiness.ConsumeBalanceAsync(string companyVkn, int amount, string reason, CancellationToken ct)
        {
            var bal = _balances.GetOrAdd(companyVkn, _ => new Balance { Remaining = 1000, Used = 0 });
            if (amount < 1) amount = 1;
            if (bal.Remaining < amount) throw new InvalidOperationException("Yetersiz kontör.");
            bal.Remaining -= amount; bal.Used += amount;
            return Task.FromResult((bal.Remaining, bal.Used));
        }

        // ---------- Mapping (Invoice -> İstek) ----------
        private OutboxInvoiceCreateRequest MapToEInvoiceRequest(Invoice inv, bool isExport)
        {
            var vkn = inv.Customer?.TaxNo ?? _opt.TestSenderVkn;
            var alias = string.IsNullOrWhiteSpace(inv.Customer?.Email) ? _opt.TestInboxAlias : inv.Customer!.Email;

            var lines = new List<InvoiceLine>();
            if (inv.InvoicesItems != null)
            {
                foreach (var it in inv.InvoicesItems)
                {
                    var name = it.Item?.Name ?? "Kalem";
                    var unit = it.Item?.Unit?.Name;
                    var unitCode = string.IsNullOrWhiteSpace(unit) ? "C62" : unit!;
                    var unitPrice = it.Price;
                    var qty = it.Quantity;
                    var vatRate = (inv.InvoicesTaxes?.FirstOrDefault()?.Rate) ?? 20m;

                    lines.Add(new InvoiceLine
                    {
                        Name = name,
                        UnitCode = unitCode,
                        UnitPrice = unitPrice,
                        Quantity = qty,
                        VatRate = vatRate
                    });
                }
            }

            decimal lineExt = inv.InvoicesItems?.Sum(x => x.Quantity * x.Price) ?? inv.Total;
            decimal taxAmount = inv.InvoicesTaxes?.Sum(x => x.Amount) ?? Math.Round(lineExt * ((inv.InvoicesTaxes?.FirstOrDefault()?.Rate ?? 20m) / 100m), 2);
            decimal taxExclusive = lineExt;
            decimal taxInclusive = taxExclusive + taxAmount;
            decimal payable = inv.Total != 0 ? inv.Total : taxInclusive;

            return new OutboxInvoiceCreateRequest
            {
                RecordType = 1,
                Status = 20,
                Type = isExport ? 2 : null,
                LocalReferenceId = inv.InvoiceNo ?? inv.Id.ToString(CultureInfo.InvariantCulture),
                AddressBook = new AddressBook
                {
                    IdentificationNumber = vkn,
                    Alias = alias,
                    Name = $"{inv.Customer?.Name} {inv.Customer?.Surname}".Trim()
                },
                Header = new InvoiceHeader
                {
                    IssueDate = DateTime.SpecifyKind(inv.InvoiceDate, DateTimeKind.Utc),
                    Currency = string.IsNullOrWhiteSpace(inv.Currency) ? "TRY" : inv.Currency,
                    Note = null
                },
                Lines = lines,
                Totals = new InvoiceTotals
                {
                    LineExtensionAmount = lineExt,
                    TaxExclusiveAmount = taxExclusive,
                    TaxInclusiveAmount = taxInclusive,
                    PayableAmount = payable
                }
            };
        }

        private OutboxInvoiceCreateRequest MapToEArchiveRequest(Invoice inv)
        {
            var req = MapToEInvoiceRequest(inv, isExport: false);
            req.RecordType = 0; // e-Arşiv
            return req;
        }

        private static byte[] BuildDespatchAdviceZip(Invoice inv)
        {
            var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2");
            var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            var doc = new XDocument(
                new XElement(ns + "DespatchAdvice",
                    new XElement(cbc + "ID", inv.InvoiceNo ?? inv.Id.ToString()),
                    new XElement(cbc + "IssueDate", inv.InvoiceDate.ToString("yyyy-MM-dd")),
                    new XElement(cac + "DespatchSupplierParty",
                        new XElement(cac + "Party",
                            new XElement(cbc + "EndpointID", inv.Customer?.Email ?? "default@local"),
                            new XElement(cac + "PartyIdentification", new XElement(cbc + "ID", inv.Customer?.TaxNo ?? "1234567803")),
                            new XElement(cac + "PartyName", new XElement(cbc + "Name", $"{inv.Customer?.Name} {inv.Customer?.Surname}".Trim()))
                        )
                    ),
                    new XElement(cac + "DespatchLine",
                        new XElement(cbc + "ID", "1"),
                        new XElement(cbc + "DeliveredQuantity", (inv.InvoicesItems?.FirstOrDefault()?.Quantity ?? 1).ToString(CultureInfo.InvariantCulture)),
                        new XElement(cac + "Item", new XElement(cbc + "Name", inv.InvoicesItems?.FirstOrDefault()?.Item?.Name ?? "Kalem"))
                    )
                )
            );

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = zip.CreateEntry("DespatchAdvice.xml");
                using var writer = new StreamWriter(entry.Open());
                writer.Write(doc.ToString(SaveOptions.DisableFormatting));
            }
            return ms.ToArray();
        }

        private OutboxDespatchUblRequest MapToDespatchUbl(Invoice inv, string? targetAlias)
        {
            var zipBytes = BuildDespatchAdviceZip(inv);
            return new OutboxDespatchUblRequest
            {
                DespatchAdviceZip = Convert.ToBase64String(zipBytes),
                LocalReferenceId = inv.InvoiceNo ?? inv.Id.ToString(),
                TargetAlias = string.IsNullOrWhiteSpace(targetAlias) ? null : targetAlias,
                Status = 20
            };
        }

        // ---------- e-Fatura / e-İhracat ----------
        async Task<HttpResult<object>> IGibBusiness.SendEInvoiceJsonAsync(Invoice inv, bool isExport, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            var payload = MapToEInvoiceRequest(inv, isExport);
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/outboxinvoice/create", ct);
            req.Content = JsonContent.Create(payload);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Fatura Gönderim (JSON)", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.SendEInvoiceUblAsync(Stream fileStream, string fileName, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/outboxinvoice", ct);
            var mp = new MultipartFormDataContent();
            mp.Add(new StreamContent(fileStream), "file", fileName);
            req.Content = mp;
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Fatura Gönderim (UBL)", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.UpdateEInvoiceStatusAsync(IEnumerable<Guid> ids, int status, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/outboxinvoice/updatestatuslist", ct);
            req.Content = JsonContent.Create(new { ids, status });
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<object>> IGibBusiness.GetEInvoiceOutboxStatusAsync(Guid ettn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxinvoice/status/{ettn}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<object>> IGibBusiness.GetEInvoiceInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct)
        {
            var url = $"/v1/inboxinvoice/list?startDate={start:yyyy-MM-dd HH:mm:ss}&endDate={end:yyyy-MM-dd HH:mm:ss}&pageIndex={pageIndex}&pageSize={pageSize}&isDesc={isDesc}";
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<byte[]>> IGibBusiness.GetEInvoiceOutboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxinvoice/pdf/{ettn}?isStandartXslt={standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }
        async Task<HttpResult<byte[]>> IGibBusiness.GetEInvoiceOutboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxinvoice/html/{ettn}?isStandartXslt={standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }
        async Task<HttpResult<byte[]>> IGibBusiness.GetEInvoiceOutboxUblAsync(Guid ettn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxinvoice/ubl/{ettn}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }
        async Task<HttpResult<byte[]>> IGibBusiness.GetEInvoiceOutboxZipAsync(Guid ettn, bool standardXslt, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxinvoice/zip/{ettn}?isStandartXslt={standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        // ---------- e-Arşiv ----------
        async Task<HttpResult<object>> IGibBusiness.SendEArchiveJsonAsync(Invoice inv, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            var payload = MapToEArchiveRequest(inv);
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/earchive/create", ct);
            req.Content = JsonContent.Create(payload);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Arşiv Gönderim (JSON)", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.SendEArchiveUblAsync(Stream fileStream, string fileName, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/earchive", ct);
            var mp = new MultipartFormDataContent();
            mp.Add(new StreamContent(fileStream), "file", fileName);
            req.Content = mp;
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Arşiv Gönderim (UBL)", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.GetEArchiveStatusAsync(Guid ettn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/earchive/status/{ettn}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<object>> IGibBusiness.CancelEArchiveAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/earchive/cancelinvoice", ct);
            req.Content = JsonContent.Create(ids);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-İrsaliye ----------
        async Task<HttpResult<object>> IGibBusiness.SendDespatchUblAsync(Invoice inv, string? targetAlias, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            var body = MapToDespatchUbl(inv, targetAlias);
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Post, "/v1/outboxdespatch", ct);
            req.Content = JsonContent.Create(body);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-İrsaliye Gönderim (UBL)", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.UpdateDespatchStatusListAsync(IEnumerable<Guid> ids, int status, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Put, "/v2/outboxdespatch/updatestatuslist", ct);
            req.Content = JsonContent.Create(new { ids, status });
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<object>> IGibBusiness.GetDespatchInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct)
        {
            var url = $"/v1/inboxdespatch/list?startDate={start:yyyy-MM-dd HH:mm:ss}&endDate={end:yyyy-MM-dd HH:mm:ss}&pageIndex={pageIndex}&pageSize={pageSize}&isDesc={isDesc}";
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-Defter ----------
        async Task<HttpResult<object>> IGibBusiness.GetEDefterPeriodListAsync(int pageIndex, int pageSize, bool isDesc, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EDefterBaseUrl, HttpMethod.Get, $"/v1/period/getperiodlist?pageIndex={pageIndex}&pageSize={pageSize}&isDesc={isDesc}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        async Task<HttpResult<object>> IGibBusiness.PostEDefterAsync(PostEDefterRequest model, Stream zipStream, string fileName, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EDefterBaseUrl, HttpMethod.Post, "/v1/period", ct);
            var mp = new MultipartFormDataContent
            {
                { new StringContent(model.StartDate), "StartDate" },
                { new StringContent(model.EndDate), "EndDate" },
                { new StringContent(model.SplitSize.ToString()), "SplitSize" },
                { new StringContent(model.TimeStamp.ToString()), "TimeStamp" },
                { new StringContent(model.WithoutTaxDetail.ToString()), "WithoutTaxDetail" }
            };
            mp.Add(new StreamContent(zipStream), "ZipFile", fileName);
            req.Content = mp;

            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok) await ((IGibBusiness)this).ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Defter Yükleme", ct);
            return res;
        }

        async Task<HttpResult<object>> IGibBusiness.SendLetterPatentsToGibAsync(CancellationToken ct)
        {
            using var req = await CreateReqAsync(_opt.EDefterBaseUrl, HttpMethod.Get, "/v1/sendletterpattentstogib", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // --------- Küçük yardımcı: faturanın ayına göre e-Defter istek oluştur (opsiyonel) ---------
        public static PostEDefterRequest MakeEDefterRequestByInvoiceMonth(Invoice inv, bool timeStamp = true, bool withoutTaxDetail = false)
        {
            var start = new DateTime(inv.InvoiceDate.Year, inv.InvoiceDate.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return new PostEDefterRequest
            {
                StartDate = start.ToString("yyyy-MM-dd"),
                EndDate = end.ToString("yyyy-MM-dd"),
                SplitSize = 50,
                TimeStamp = timeStamp,
                WithoutTaxDetail = withoutTaxDetail
            };
        }

        Task<HttpResult<object>> IGibBusiness.PostEDefterAsync(Interface.IGibBusiness.PostEDefterRequest model, Stream zipStream, string fileName, bool consumeKontor, string? kontorVkn, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        public sealed class PostEDefterRequest
        {
            public string StartDate { get; set; } = default!; // yyyy-MM-dd
            public string EndDate { get; set; } = default!;
            public int SplitSize { get; set; } = 50;
            public bool TimeStamp { get; set; }
            public bool WithoutTaxDetail { get; set; }
        }
    }
}
