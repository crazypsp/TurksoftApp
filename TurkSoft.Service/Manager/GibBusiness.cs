using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    public sealed class GibBusiness : IGibBusiness
    {
        // ---- Ayarlar ----
        public sealed class Options
        {
            public string EFaturaBaseUrl { get; init; } = "https://efaturaservicetest.isim360.com";
            public string EIrsaliyeBaseUrl { get; init; } = "https://eirsaliyeservicetest.isim360.com";
            public string EDefterBaseUrl { get; init; } = "https://edefterservicetest.isim360.com";

            // 🔴 BURAYA test portalinden oluşturduğun API KEY'i yazacaksın
            // Örnek: "abc123xyz..."
            public string ApiKey { get; init; } = "EcwbRm3WbGk2tRpNGwlu4j7UbuPiVZwWCBdhLIx3Bbo=";

            public string TestSenderVkn { get; init; } = "1234567803";
            public string TestInboxAlias { get; init; } = "urn:mail:defaulttest3pk@medyasoft.com.tr";
        }

        // ---- DTO'lar ----

        // E-Fatura JSON modeli – Postman "E-Fatura Json Model Gönderim" ile birebir
        private sealed class OutboxInvoiceCreateRequest
        {
            [JsonPropertyName("recordType")] public int RecordType { get; set; } = 1; // 1: e-Fatura
            [JsonPropertyName("status")] public int Status { get; set; } = 0;
            [JsonPropertyName("xsltCode")] public string? XsltCode { get; set; }
            [JsonPropertyName("localReferenceId")] public string? LocalReferenceId { get; set; }
            [JsonPropertyName("useManualInvoiceId")] public bool UseManualInvoiceId { get; set; }
            [JsonPropertyName("note")] public string? Note { get; set; }
            [JsonPropertyName("notes")] public List<string>? Notes { get; set; }

            [JsonPropertyName("addressBook")] public AddressBook AddressBook { get; set; } = new();
            [JsonPropertyName("generalInfoModel")] public GeneralInfoModel GeneralInfoModel { get; set; } = new();
            [JsonPropertyName("invoiceLines")] public List<InvoiceLine> InvoiceLines { get; set; } = new();
        }

        private sealed class AddressBook
        {
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("receiverPersonSurName")] public string? ReceiverPersonSurName { get; set; }
            [JsonPropertyName("identificationNumber")] public string? IdentificationNumber { get; set; }
            [JsonPropertyName("alias")] public string? Alias { get; set; }
            [JsonPropertyName("registerNumber")] public string? RegisterNumber { get; set; }
            [JsonPropertyName("receiverStreet")] public string? ReceiverStreet { get; set; }
            [JsonPropertyName("receiverBuildingName")] public string? ReceiverBuildingName { get; set; }
            [JsonPropertyName("receiverBuildingNumber")] public string? ReceiverBuildingNumber { get; set; }
            [JsonPropertyName("receiverDoorNumber")] public string? ReceiverDoorNumber { get; set; }
            [JsonPropertyName("receiverSmallTown")] public string? ReceiverSmallTown { get; set; }
            [JsonPropertyName("receiverDistrict")] public string? ReceiverDistrict { get; set; }
            [JsonPropertyName("receiverZipCode")] public string? ReceiverZipCode { get; set; }
            [JsonPropertyName("receiverCity")] public string? ReceiverCity { get; set; }
            [JsonPropertyName("receiverCountry")] public string? ReceiverCountry { get; set; }
            [JsonPropertyName("receiverPhoneNumber")] public string? ReceiverPhoneNumber { get; set; }
            [JsonPropertyName("receiverFaxNumber")] public string? ReceiverFaxNumber { get; set; }
            [JsonPropertyName("receiverEmail")] public string? ReceiverEmail { get; set; }
            [JsonPropertyName("receiverWebSite")] public string? ReceiverWebSite { get; set; }
            [JsonPropertyName("receiverTaxOffice")] public string? ReceiverTaxOffice { get; set; }
        }

        private sealed class GeneralInfoModel
        {
            [JsonPropertyName("ettn")] public Guid? Ettn { get; set; }
            [JsonPropertyName("prefix")] public string? Prefix { get; set; }
            [JsonPropertyName("invoiceNumber")] public string? InvoiceNumber { get; set; }
            [JsonPropertyName("invoiceProfileType")] public int InvoiceProfileType { get; set; }  // 0:Temel, 1:Ticari
            [JsonPropertyName("issueDate")] public DateTime IssueDate { get; set; }
            [JsonPropertyName("type")] public int Type { get; set; }              // 1:SATIS, 2:IADE, 7:IHRACKAYITLI vb.
            [JsonPropertyName("returnInvoiceNumber")] public string? ReturnInvoiceNumber { get; set; }
            [JsonPropertyName("returnInvoiceDate")] public DateTime? ReturnInvoiceDate { get; set; }
            [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = "TRY";
            [JsonPropertyName("exchangeRate")] public decimal ExchangeRate { get; set; }
        }

        private sealed class InvoiceLine
        {
            [JsonPropertyName("inventoryCard")] public string InventoryCard { get; set; } = default!;
            [JsonPropertyName("amount")] public decimal Amount { get; set; }
            [JsonPropertyName("discountAmount")] public decimal DiscountAmount { get; set; }
            [JsonPropertyName("lineAmount")] public decimal LineAmount { get; set; }
            [JsonPropertyName("vatAmount")] public decimal VatAmount { get; set; }
            [JsonPropertyName("unitCode")] public string UnitCode { get; set; } = "C62";
            [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; set; }
            [JsonPropertyName("discountRate")] public decimal DiscountRate { get; set; }
            [JsonPropertyName("vatRate")] public decimal VatRate { get; set; }
            [JsonPropertyName("vatExemptionReasonCode")] public string? VatExemptionReasonCode { get; set; }

            [JsonPropertyName("description")] public string? Description { get; set; }
            [JsonPropertyName("note")] public string? Note { get; set; }
            [JsonPropertyName("sellersItemIdentification")] public string? SellersItemIdentification { get; set; }
            [JsonPropertyName("buyersItemIdentification")] public string? BuyersItemIdentification { get; set; }
            [JsonPropertyName("manufacturersItemIdentification")] public string? ManufacturersItemIdentification { get; set; }

            [JsonPropertyName("taxes")] public List<TaxLine>? Taxes { get; set; }
        }

        private sealed class TaxLine
        {
            [JsonPropertyName("taxName")] public string TaxName { get; set; } = default!;
            [JsonPropertyName("isNegative")] public bool IsNegative { get; set; }
            [JsonPropertyName("taxTypeCode")] public string TaxTypeCode { get; set; } = default!;
            [JsonPropertyName("taxRate")] public decimal TaxRate { get; set; }
            [JsonPropertyName("taxAmount")] public decimal TaxAmount { get; set; }
        }


        private sealed class InvoiceHeader
        {
            public DateTime IssueDate { get; set; } = DateTime.UtcNow;
            public string Currency { get; set; } = "TRY";
            public string? Note { get; set; }
        }       

        private sealed class InvoiceTotals
        {
            public decimal LineExtensionAmount { get; set; }
            public decimal TaxExclusiveAmount { get; set; }
            public decimal TaxInclusiveAmount { get; set; }
            public decimal PayableAmount { get; set; }
        }

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




        // ---- E-Arşiv JSON DTO'ları (Post_Earchive_Invoice_Json'a birebir uyumlu) ----

        private sealed class EArchiveInvoiceCreateRequest
        {
            [JsonPropertyName("recordType")] public int RecordType { get; set; } = 0;  // 0: e-Arşiv
            [JsonPropertyName("status")] public int Status { get; set; } = 20;
            [JsonPropertyName("isNew")] public bool IsNew { get; set; } = true;
            [JsonPropertyName("localReferenceId")] public string? LocalReferenceId { get; set; }
            [JsonPropertyName("useManualInvoiceId")] public bool UseManualInvoiceId { get; set; } = false;
            [JsonPropertyName("note")] public string? Note { get; set; }

            [JsonPropertyName("generalInfoModel")] public EArchiveGeneralInfoModel GeneralInfoModel { get; set; } = new();
            [JsonPropertyName("addressBook")] public EArchiveAddressBook AddressBook { get; set; } = new();
            [JsonPropertyName("invoiceLines")] public List<EArchiveInvoiceLine> InvoiceLines { get; set; } = new();
            [JsonPropertyName("archiveInfoModel")] public EArchiveArchiveInfoModel ArchiveInfoModel { get; set; } = new();
            [JsonPropertyName("paymentMeansModel")] public EArchivePaymentMeansModel PaymentMeansModel { get; set; } = new();
            [JsonPropertyName("ublSettingsModel")] public EArchiveUblSettingsModel UblSettingsModel { get; set; } = new();
            [JsonPropertyName("eArsivInfo")] public EArchiveInfo EArsivInfo { get; set; } = new();
        }

        private sealed class EArchiveGeneralInfoModel
        {
            [JsonPropertyName("ettn")] public Guid? Ettn { get; set; }
            [JsonPropertyName("invoiceProfileType")] public int InvoiceProfileType { get; set; }  // 4: e-Arşiv
            [JsonPropertyName("issueDate")] public DateTime IssueDate { get; set; }
            [JsonPropertyName("type")] public int Type { get; set; }                 // 1: SATIS vs.
            [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = "TRY";
            [JsonPropertyName("exchangeRate")] public decimal ExchangeRate { get; set; }
            [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }
            [JsonPropertyName("invoiceNumber")] public string? InvoiceNumber { get; set; }
            [JsonPropertyName("issueTime")] public DateTime? IssueTime { get; set; }
            [JsonPropertyName("prefix")] public string? Prefix { get; set; }
            [JsonPropertyName("returnInvoiceDate")] public DateTime? ReturnInvoiceDate { get; set; }
            [JsonPropertyName("returnInvoiceNumber")] public string? ReturnInvoiceNumber { get; set; }
            [JsonPropertyName("slipNumber")] public string? SlipNumber { get; set; }
        }

        private sealed class EArchiveAddressBook
        {
            [JsonPropertyName("identificationNumber")] public string? IdentificationNumber { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("receiverBuildingName")] public string? ReceiverBuildingName { get; set; }
            [JsonPropertyName("receiverPersonSurName")] public string? ReceiverPersonSurName { get; set; }
            [JsonPropertyName("receiverNumber")] public string? ReceiverNumber { get; set; }
            [JsonPropertyName("receiverStreet")] public string? ReceiverStreet { get; set; }
            [JsonPropertyName("receiverEmail")] public string? ReceiverEmail { get; set; }
            [JsonPropertyName("receiverDistrict")] public string? ReceiverDistrict { get; set; }
            [JsonPropertyName("receiverCity")] public string? ReceiverCity { get; set; }
            [JsonPropertyName("receiverCountry")] public string? ReceiverCountry { get; set; }
            [JsonPropertyName("registerNumber")] public string? RegisterNumber { get; set; }
            [JsonPropertyName("receiverTaxOffice")] public string? ReceiverTaxOffice { get; set; }
            [JsonPropertyName("receiverPhoneNumber")] public string? ReceiverPhoneNumber { get; set; }
        }

        private sealed class EArchiveInvoiceLine
        {
            [JsonPropertyName("inventoryCard")] public string InventoryCard { get; set; } = default!;
            [JsonPropertyName("disableVatExemption")] public bool DisableVatExemption { get; set; }
            [JsonPropertyName("amount")] public decimal Amount { get; set; }
            [JsonPropertyName("unitCode")] public string UnitCode { get; set; } = "C62";
            [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; set; }
            [JsonPropertyName("vatRate")] public decimal VatRate { get; set; }
            [JsonPropertyName("discountAmount")] public decimal DiscountAmount { get; set; }
            [JsonPropertyName("vatAmount")] public decimal VatAmount { get; set; }
            [JsonPropertyName("lineExtensionAmount")] public decimal LineExtensionAmount { get; set; }
            [JsonPropertyName("vatExemptionReasonCode")] public string? VatExemptionReasonCode { get; set; }
        }

        private sealed class EArchiveArchiveInfoModel
        {
            [JsonPropertyName("isInternetSale")] public bool IsInternetSale { get; set; }
            [JsonPropertyName("shipmentDate")] public DateTime? ShipmentDate { get; set; }
            [JsonPropertyName("shipmentSendType")] public string? ShipmentSendType { get; set; }
            [JsonPropertyName("shipmentSenderName")] public string? ShipmentSenderName { get; set; }
            [JsonPropertyName("shipmentSenderSurname")] public string? ShipmentSenderSurname { get; set; }
            [JsonPropertyName("shipmentSenderTcknVkn")] public string? ShipmentSenderTcknVkn { get; set; }
            [JsonPropertyName("subscriptionType")] public string? SubscriptionType { get; set; }
            [JsonPropertyName("subscriptionTypeNumber")] public string? SubscriptionTypeNumber { get; set; }
            [JsonPropertyName("websiteUrl")] public string? WebsiteUrl { get; set; }
        }

        private sealed class EArchivePaymentMeansModel
        {
            [JsonPropertyName("instructionNote")] public string? InstructionNote { get; set; }
            [JsonPropertyName("payeeFinancialAccountCurrencyCode")] public string? PayeeFinancialAccountCurrencyCode { get; set; }
            [JsonPropertyName("payeeFinancialAccountId")] public string? PayeeFinancialAccountId { get; set; }
            [JsonPropertyName("paymentChannelCode")] public string? PaymentChannelCode { get; set; }
            [JsonPropertyName("paymentDueDate")] public DateTime? PaymentDueDate { get; set; }
            [JsonPropertyName("paymentMeansCode")] public int PaymentMeansCode { get; set; }
        }

        private sealed class EArchiveUblSettingsModel
        {
            [JsonPropertyName("useCalculatedVatAmount")] public bool UseCalculatedVatAmount { get; set; }
            [JsonPropertyName("hideDespatchMessage")] public bool HideDespatchMessage { get; set; }
        }

        private sealed class EArchiveInfo
        {
            [JsonPropertyName("sendEMail")] public bool SendEmail { get; set; }
            [JsonPropertyName("eMailAddress")] public string? EmailAddress { get; set; }
        }




        // ---- Alanlar ----
        private readonly Options _opt;
        private readonly HttpClient _http;
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

        // ---------- Yardımcı HTTP ----------
        private Task<HttpRequestMessage> CreateReqAsync(string baseUrl, HttpMethod method, string path, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                throw new InvalidOperationException("GibBusiness.Options.ApiKey boş. portaltest.isim360.com üzerinden oluşturduğunuz API key'i Options.ApiKey içine yazın.");

            var req = new HttpRequestMessage(method, new Uri(new Uri(baseUrl), path));
            // Turkcell / ePlatform servisleri APIKEY'i buradan bekliyor
            req.Headers.Add("x-api-key", _opt.ApiKey);
            return Task.FromResult(req);
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
        public Task<(int remaining, int used)> GetBalanceAsync(string companyVkn, CancellationToken ct = default)
        {
            var bal = _balances.GetOrAdd(companyVkn, _ => new Balance { Remaining = 1000, Used = 0 });
            return Task.FromResult((bal.Remaining, bal.Used));
        }

        public Task<(int remaining, int used)> ConsumeBalanceAsync(string companyVkn, int amount, string reason, CancellationToken ct = default)
        {
            var bal = _balances.GetOrAdd(companyVkn, _ => new Balance { Remaining = 1000, Used = 0 });
            if (amount < 1) amount = 1;
            if (bal.Remaining < amount) throw new InvalidOperationException("Yetersiz kontör.");
            bal.Remaining -= amount;
            bal.Used += amount;
            return Task.FromResult((bal.Remaining, bal.Used));
        }

        // ---------- Mapping (Invoice -> JSON) ----------
        // ---------- Mapping (Invoice -> JSON) ----------
        private OutboxInvoiceCreateRequest MapToEInvoiceRequest(Invoice inv, bool isExport)
        {
            // Müşteri & VKN
            var customer = inv.Customer;
            var vkn = customer?.TaxNo ?? _opt.TestSenderVkn;

            // Alias – test ortamında genelde sabit
            var alias = _opt.TestInboxAlias;

            // Satırları hesapla
            var lines = new List<InvoiceLine>();
            decimal lineExtTotal = 0m;
            decimal vatTotal = 0m;

            // Eğer invoice üzerinde vergi satırı yoksa:
            var defaultVatRate = inv.InvoicesTaxes != null && inv.InvoicesTaxes.Any()
                ? inv.InvoicesTaxes.First().Rate
                : (isExport ? 0m : 18m);

            if (inv.InvoicesItems != null && inv.InvoicesItems.Any())
            {
                foreach (var it in inv.InvoicesItems)
                {
                    var qty = it.Quantity;
                    var unitPrice = it.Price;
                    var lineAmount = it.Total != 0 ? it.Total : qty * unitPrice;

                    var vatRate = defaultVatRate;
                    var vatAmount = Math.Round(lineAmount * vatRate / 100m, 2);

                    lineExtTotal += lineAmount;
                    vatTotal += vatAmount;

                    // Birim kodu – Unit.ShortName varsa onu, yoksa Name, o da yoksa C62
                    var unitCode =
                        it.Item?.Unit?.ShortName ??
                        it.Item?.Unit?.Name ??
                        "C62";

                    // 🔴 SADECE İHRACAT İÇİN: KDV 0 ise istisna kodu zorunlu
                    string? vatExemptionReasonCode = null;
                    if (isExport && vatRate == 0m)
                    {
                        // Dokümana uygun bir ihracat/istisna kodu
                        // Örnek: 301 – İhracat KDV istisnası
                        vatExemptionReasonCode = "301";
                    }

                    lines.Add(new InvoiceLine
                    {
                        InventoryCard = it.Item?.Name ?? "Kalem",
                        Amount = qty,
                        DiscountAmount = 0m,
                        LineAmount = lineAmount,
                        VatAmount = vatAmount,
                        UnitCode = unitCode,
                        UnitPrice = unitPrice,
                        DiscountRate = 0m,
                        VatRate = vatRate,
                        VatExemptionReasonCode = vatExemptionReasonCode,

                        Description = null,
                        Note = null,
                        SellersItemIdentification = it.Item?.Code,
                        BuyersItemIdentification = null,
                        ManufacturersItemIdentification = null
                    });
                }
            }

            if (lineExtTotal == 0m)
                lineExtTotal = inv.Total;

            if (vatTotal == 0m && inv.InvoicesTaxes != null && inv.InvoicesTaxes.Any())
                vatTotal = inv.InvoicesTaxes.Sum(t => t.Amount);

            var taxExclusive = lineExtTotal;
            var taxInclusive = taxExclusive + vatTotal;
            var payable = inv.Total != 0 ? inv.Total : taxInclusive;

            var issueDate = inv.InvoiceDate == default ? DateTime.Now : inv.InvoiceDate;

            // Fatura tipi – SATIŞ / İHRACAT vs.
            // 1: SATIS, 2: IADE, 7: IHRACKAYITLI vb. – dokümana göre ayarlayabilirsin
            var type = isExport ? 7 : 1;

            var currency = string.IsNullOrWhiteSpace(inv.Currency) ? "TRY" : inv.Currency;
            var exchangeRate = currency == "TRY" ? 0m : 1m; // TRY dışında gerçek kur set edebilirsin

            return new OutboxInvoiceCreateRequest
            {
                // Üst alanlar
                RecordType = 1, // e-Fatura
                Status = 0,
                XsltCode = null,
                LocalReferenceId = !string.IsNullOrWhiteSpace(inv.InvoiceNo)
                    ? inv.InvoiceNo
                    : inv.Id.ToString(CultureInfo.InvariantCulture),
                UseManualInvoiceId = false,
                Note = null,
                Notes = new List<string>(),

                // Adres bilgileri – 422'de şikâyet edilen tüm alanlar dolduruluyor
                AddressBook = new AddressBook
                {
                    Name = $"{customer?.Name} {customer?.Surname}".Trim(),
                    ReceiverPersonSurName = customer?.Surname,
                    IdentificationNumber = vkn,
                    Alias = alias,

                    RegisterNumber = "11112222333444",
                    ReceiverStreet = "Bulvar / cadde / sokak",
                    ReceiverBuildingName = "Bina adı",
                    ReceiverBuildingNumber = "1",
                    ReceiverDoorNumber = "11",
                    ReceiverSmallTown = "Kasaba / Köy",
                    ReceiverDistrict = "Üsküdar",   // 🔴 boş bırakmıyoruz
                    ReceiverZipCode = "34000",
                    ReceiverCity = "İstanbul",      // 🔴 boş bırakmıyoruz
                    ReceiverCountry = "Türkiye",    // 🔴 boş bırakmıyoruz
                    ReceiverPhoneNumber = customer?.Phone ?? "02121234567",
                    ReceiverFaxNumber = "02129876543",
                    ReceiverEmail = customer?.Email ?? "test@example.com",
                    ReceiverWebSite = "www.testfirma.com",
                    ReceiverTaxOffice = customer?.TaxOffice ?? "Üsküdar vergi dairesi"
                },

                // Genel bilgiler
                GeneralInfoModel = new GeneralInfoModel
                {
                    Ettn = null,
                    Prefix = "",                     // gerekirse TST gibi prefix verebilirsin
                    InvoiceNumber = null,            // UseManualInvoiceId=false olduğu için null
                    InvoiceProfileType = 1,          // 1: Ticari – böylece /einvoice/response ile yanıt verebilirsin
                    IssueDate = issueDate,
                    Type = type,
                    ReturnInvoiceNumber = null,
                    ReturnInvoiceDate = null,
                    CurrencyCode = currency,
                    ExchangeRate = exchangeRate
                },

                // Satır listesi
                InvoiceLines = lines
            };
        }


        private EArchiveInvoiceCreateRequest MapToEArchiveRequest(Invoice inv)
        {
            var customer = inv.Customer;

            // 🔴 E-Arşiv için: e-fatura kullanıcısı VKN'sine (1234567803) e-arşiv kesilmez.
            // Test senaryosu için: bu durumda "nihai tüketici" örneği olan 1123581321'e çeviriyoruz.
            var receiverId = customer?.TaxNo;

            if (string.IsNullOrWhiteSpace(receiverId) ||
                receiverId == "1234567803" ||      // mailde/testte verilen e-fatura VKN
                receiverId == _opt.TestSenderVkn)  // Options içindeki test VKN'i de büyük ihtimalle bu
            {
                receiverId = "1123581321";         // Turkcell dokümanındaki e-Arşiv nihai tüketici örneği
            }

            var lines = new List<EArchiveInvoiceLine>();
            decimal lineExtTotal = 0m;
            decimal vatTotal = 0m;

            var defaultVatRate = inv.InvoicesTaxes != null && inv.InvoicesTaxes.Any()
                ? inv.InvoicesTaxes.First().Rate
                : 18m;

            if (inv.InvoicesItems != null && inv.InvoicesItems.Any())
            {
                foreach (var it in inv.InvoicesItems)
                {
                    var qty = it.Quantity;
                    var unitPrice = it.Price;
                    var lineAmount = it.Total != 0 ? it.Total : qty * unitPrice;

                    var vatRate = defaultVatRate;
                    var vatAmount = Math.Round(lineAmount * vatRate / 100m, 2);

                    lineExtTotal += lineAmount;
                    vatTotal += vatAmount;

                    var unitCode =
                        it.Item?.Unit?.ShortName ??
                        it.Item?.Unit?.Name ??
                        "C62";

                    // 🔴 KDV 0 ise istisna kodu zorunlu – burada 301 kullanıyoruz (ihracat/istisna örneği)
                    string vatExemptionReasonCode = "";
                    if (vatRate == 0m)
                    {
                        vatExemptionReasonCode = "301";
                    }

                    lines.Add(new EArchiveInvoiceLine
                    {
                        InventoryCard = it.Item?.Name ?? "Kalem",
                        DisableVatExemption = false,
                        Amount = qty,
                        UnitCode = unitCode,
                        UnitPrice = unitPrice,
                        VatRate = vatRate,
                        DiscountAmount = 0m,
                        VatAmount = vatAmount,
                        LineExtensionAmount = lineAmount,
                        VatExemptionReasonCode = vatExemptionReasonCode
                    });
                }
            }

            if (lineExtTotal == 0m)
                lineExtTotal = inv.Total;

            if (vatTotal == 0m && inv.InvoicesTaxes != null && inv.InvoicesTaxes.Any())
                vatTotal = inv.InvoicesTaxes.Sum(t => t.Amount);

            var payable = inv.Total != 0 ? inv.Total : lineExtTotal + vatTotal;

            var issueDate = inv.InvoiceDate == default ? DateTime.Now : inv.InvoiceDate;
            var currency = string.IsNullOrWhiteSpace(inv.Currency) ? "TRY" : inv.Currency!;
            var exchangeRate = currency == "TRY" ? 0m : 1m; // TRY harici için gerçek kur koyabilirsin

            return new EArchiveInvoiceCreateRequest
            {
                RecordType = 0,  // e-Arşiv
                Status = 20,     // Postman örneği
                IsNew = true,
                LocalReferenceId = !string.IsNullOrWhiteSpace(inv.InvoiceNo)
                    ? inv.InvoiceNo
                    : inv.Id.ToString(CultureInfo.InvariantCulture),
                UseManualInvoiceId = false,
                Note = null,

                GeneralInfoModel = new EArchiveGeneralInfoModel
                {
                    Ettn = null,
                    InvoiceProfileType = 4,          // Postman'de e-Arşiv için 4
                    IssueDate = issueDate,
                    Type = 1,                        // 1: SATIS
                    CurrencyCode = currency,
                    ExchangeRate = exchangeRate,
                    TotalAmount = payable,
                    InvoiceNumber = "",
                    IssueTime = issueDate,
                    Prefix = null,
                    ReturnInvoiceDate = null,
                    ReturnInvoiceNumber = null,
                    SlipNumber = null
                },

                AddressBook = new EArchiveAddressBook
                {
                    IdentificationNumber = receiverId,  // 🔴 burada vkn yerine düzeltilmiş receiverId'yi kullanıyoruz
                    Name = $"{customer?.Name} {customer?.Surname}".Trim(),
                    ReceiverBuildingName = null,
                    ReceiverPersonSurName = customer?.Surname,
                    ReceiverNumber = customer?.Phone,
                    ReceiverStreet = "",
                    ReceiverEmail = customer?.Email ?? "abb@gmail.com",
                    ReceiverDistrict = "PENDİK",        // 🔴 Boş değil
                    ReceiverCity = "İSTANBUL",          // 🔴 Boş değil
                    ReceiverCountry = "Türkiye",        // 🔴 Boş değil
                    RegisterNumber = null,
                    ReceiverTaxOffice = customer?.TaxOffice ?? "",
                    ReceiverPhoneNumber = customer?.Phone ?? ""
                },

                InvoiceLines = lines,

                ArchiveInfoModel = new EArchiveArchiveInfoModel
                {
                    IsInternetSale = false,
                    ShipmentDate = null,
                    ShipmentSendType = null,
                    ShipmentSenderName = null,
                    ShipmentSenderSurname = null,
                    ShipmentSenderTcknVkn = null,
                    SubscriptionType = null,
                    SubscriptionTypeNumber = null,
                    WebsiteUrl = null
                },

                PaymentMeansModel = new EArchivePaymentMeansModel
                {
                    InstructionNote = null,
                    PayeeFinancialAccountCurrencyCode = currency,
                    PayeeFinancialAccountId = null,
                    PaymentChannelCode = null,
                    PaymentDueDate = null,
                    PaymentMeansCode = 0
                },

                UblSettingsModel = new EArchiveUblSettingsModel
                {
                    UseCalculatedVatAmount = true,
                    HideDespatchMessage = false
                },

                EArsivInfo = new EArchiveInfo
                {
                    SendEmail = true,
                    EmailAddress = customer?.Email ?? "abb@gmail.com"
                }
            };
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
                            new XElement(cac + "PartyIdentification",
                                new XElement(cbc + "ID", inv.Customer?.TaxNo ?? "1234567803")),
                            new XElement(cac + "PartyName",
                                new XElement(cbc + "Name", $"{inv.Customer?.Name} {inv.Customer?.Surname}".Trim()))
                        )
                    ),
                    new XElement(cac + "DespatchLine",
                        new XElement(cbc + "ID", "1"),
                        new XElement(cbc + "DeliveredQuantity",
                            (inv.InvoicesItems?.FirstOrDefault()?.Quantity ?? 1).ToString(CultureInfo.InvariantCulture)),
                        new XElement(cac + "Item",
                            new XElement(cbc + "Name", inv.InvoicesItems?.FirstOrDefault()?.Item?.Name ?? "Kalem"))
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

        // ---------- StaticList ----------
        public async Task<HttpResult<object>> GetStaticListUnitAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/unit", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetStaticListTaxExemptionReasonsAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/taxexemptionreason", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetStaticListWithHoldingAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/withholding", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetStaticListTaxTypeCodeAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/taxtypecode", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetStaticListTaxOfficeAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/taxoffice", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetStaticListCountryAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v1/staticlist/country", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-Fatura Outbox ----------
        public async Task<HttpResult<object>> SendEInvoiceJsonAsync(Invoice inv, bool isExport = false, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
        {
            var payload = MapToEInvoiceRequest(inv, isExport);
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/outboxinvoice/create", ct);
            req.Content = JsonContent.Create(payload);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Fatura Gönderim (JSON)", ct);
            return res;
        }

        public async Task<HttpResult<object>> SendEInvoiceUblAsync(Stream fileStream, string fileName,
            int appType, int status, bool useManualInvoiceId,
            string? targetAlias, bool? useFirstAlias, string? prefix,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v2/outboxinvoice", ct);
            var mp = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "invoiceFile", fileName },
                { new StringContent(appType.ToString()), "appType" },
                { new StringContent(status.ToString()), "status" },
                { new StringContent(useManualInvoiceId.ToString()), "useManualInvoiceId" }
            };

            if (!string.IsNullOrWhiteSpace(targetAlias)) mp.Add(new StringContent(targetAlias), "targetAlias");
            if (useFirstAlias.HasValue) mp.Add(new StringContent(useFirstAlias.Value.ToString()), "useFirstAlias");
            if (!string.IsNullOrWhiteSpace(prefix)) mp.Add(new StringContent(prefix), "prefix");
            if (!string.IsNullOrWhiteSpace(localReferenceId)) mp.Add(new StringContent(localReferenceId), "localReferenceId");
            if (checkLocalReferenceId.HasValue) mp.Add(new StringContent(checkLocalReferenceId.Value.ToString()), "checkLocalReferenceId");
            if (!string.IsNullOrWhiteSpace(xsltCode)) mp.Add(new StringContent(xsltCode), "xsltCode");

            req.Content = mp;
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Fatura Gönderim (UBL)", ct);
            return res;
        }

        public async Task<HttpResult<object>> UpdateEInvoiceUblAsync(Guid ettn, Stream fileStream, string fileName,
            int appType, int status, bool useManualInvoiceId,
            string? targetAlias, bool? useFirstAlias,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode,
            bool consumeKontor = false, string? kontorVkn = null, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, $"/v2/outboxinvoice/{ettn}", ct);
            var mp = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "invoiceFile", fileName },
                { new StringContent(appType.ToString()), "appType" },
                { new StringContent(status.ToString()), "status" },
                { new StringContent(useManualInvoiceId.ToString()), "useManualInvoiceId" }
            };

            if (!string.IsNullOrWhiteSpace(targetAlias)) mp.Add(new StringContent(targetAlias), "targetAlias");
            if (useFirstAlias.HasValue) mp.Add(new StringContent(useFirstAlias.Value.ToString()), "useFirstAlias");
            if (!string.IsNullOrWhiteSpace(localReferenceId)) mp.Add(new StringContent(localReferenceId), "localReferenceId");
            if (checkLocalReferenceId.HasValue) mp.Add(new StringContent(checkLocalReferenceId.Value.ToString()), "checkLocalReferenceId");
            if (!string.IsNullOrWhiteSpace(xsltCode)) mp.Add(new StringContent(xsltCode), "xsltCode");

            req.Content = mp;
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Fatura Güncelleme (UBL)", ct);
            return res;
        }

        public async Task<HttpResult<object>> UpdateEInvoiceJsonAsync(Guid ettn, object body, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, $"/v1/outboxinvoice/update/{ettn}", ct);
            req.Content = JsonContent.Create(body);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> UpdateEInvoiceStatusAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/outboxinvoice/updatestatuslist", ct);
            req.Content = JsonContent.Create(new { ids, status });
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetEInvoiceOutboxStatusAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/outboxinvoice/{ettn}/status", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceOutboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/outboxinvoice/{ettn}/pdf/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceOutboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/outboxinvoice/{ettn}/html/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceOutboxUblAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/outboxinvoice/{ettn}/ubl", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetOutboxInvoicesWithNullLocalReferencesAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var url = $"/v2/outboxinvoice/withnulllocalreferences?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}";
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetOutboxInvoiceReasonAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/outboxInvoice/invoicereason/{ettn}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-Fatura Inbox ----------
        public async Task<HttpResult<object>> GetEInvoiceInboxAsync(DateTime start, DateTime end,
            int pageIndex, int pageSize, bool isNew, CancellationToken ct = default)
        {
            var url =
                $"/v1/inboxInvoice/list?pageIndex={pageIndex}&pageSize={pageSize}&isNew={isNew}" +
                $"&startDate={start:yyyy-MM-dd HH:mm:ss}&endDate={end:yyyy-MM-dd HH:mm:ss}";
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceInboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get,
                $"/v2/inboxinvoice/{ettn}/html/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceInboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get,
                $"/v2/inboxinvoice/{ettn}/pdf/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEInvoiceInboxUblAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get,
                $"/v2/inboxinvoice/{ettn}/ubl", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetEInvoiceInboxStatusAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get,
                $"/v2/inboxinvoice/{ettn}/status", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> SendInvoiceResponseAsync(InvoiceResponseRequest request, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/invoiceresponse", ct);
            req.Content = JsonContent.Create(request);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> RetryInvoiceResponseListAsync(IEnumerable<Guid> invoiceIds, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/invoiceresponse/retryinvoiceresponselist", ct);
            req.Content = JsonContent.Create(invoiceIds);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> UpdateInboxInvoiceIsNewAsync(IEnumerable<InboxInvoiceIsNewUpdate> items, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/inboxinvoice/updateisnew", ct);
            req.Content = JsonContent.Create(items);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-Arşiv ----------
        public async Task<HttpResult<object>> SendEArchiveJsonAsync(
    Invoice inv,
    bool consumeKontor = true,
    string? kontorVkn = null,
    CancellationToken ct = default)
        {
            var payload = MapToEArchiveRequest(inv);

            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v2/earchive/create", ct);
            req.Content = JsonContent.Create(payload);

            var res = await SendAsync<object>(_http, req, ct);

            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Arşiv Gönderim (JSON)", ct);

            return res;
        }

        public async Task<HttpResult<object>> SendEArchiveJsonRawAsync(object body, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v2/earchive/create", ct);
            req.Content = JsonContent.Create(body);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Arşiv Gönderim (JSON Raw)", ct);
            return res;
        }

        public async Task<HttpResult<object>> SendEArchiveUblAsync(Stream fileStream, string fileName,
            int status, bool useManualInvoiceId, bool sendEmail,
            string? emailAddress, string? prefix, string? localReferenceId,
            bool? checkLocalReferenceId, string? xsltCode, bool? allowOldEArsivCustomer,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v2/earchive", ct);
            var mp = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "invoiceFile", fileName },
                { new StringContent(status.ToString()), "status" },
                { new StringContent(useManualInvoiceId.ToString()), "useManualInvoiceId" },
                { new StringContent(sendEmail.ToString()), "sendEMail" }
            };

            if (!string.IsNullOrWhiteSpace(emailAddress)) mp.Add(new StringContent(emailAddress), "eMailAddress");
            if (!string.IsNullOrWhiteSpace(prefix)) mp.Add(new StringContent(prefix), "prefix");
            if (!string.IsNullOrWhiteSpace(localReferenceId)) mp.Add(new StringContent(localReferenceId), "localReferenceId");
            if (checkLocalReferenceId.HasValue) mp.Add(new StringContent(checkLocalReferenceId.Value.ToString()), "checkLocalReferenceId");
            if (!string.IsNullOrWhiteSpace(xsltCode)) mp.Add(new StringContent(xsltCode), "xsltCode");
            if (allowOldEArsivCustomer.HasValue) mp.Add(new StringContent(allowOldEArsivCustomer.Value.ToString()), "allowOldEArsivCustomer");

            req.Content = mp;
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Arşiv Gönderim (UBL)", ct);
            return res;
        }

        public async Task<HttpResult<object>> UpdateEArchiveUblAsync(Guid id, Stream fileStream, string fileName,
            int status, bool sendEmail, string emailAddress,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode, bool? allowOldEArsivCustomer,
            CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, $"/v2/earchive/{id}", ct);
            var mp = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "invoiceFile", fileName },
                { new StringContent(status.ToString()), "status" },
                { new StringContent(sendEmail.ToString()), "sendEMail" },
                { new StringContent(emailAddress), "eMailAddress" }
            };

            if (!string.IsNullOrWhiteSpace(localReferenceId)) mp.Add(new StringContent(localReferenceId), "localReferenceId");
            if (checkLocalReferenceId.HasValue) mp.Add(new StringContent(checkLocalReferenceId.Value.ToString()), "checkLocalReferenceId");
            if (!string.IsNullOrWhiteSpace(xsltCode)) mp.Add(new StringContent(xsltCode), "xsltCode");
            if (allowOldEArsivCustomer.HasValue) mp.Add(new StringContent(allowOldEArsivCustomer.Value.ToString()), "allowOldEArsivCustomer");

            req.Content = mp;
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetEArchiveStatusAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/earchive/{ettn}/status", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEArchivePdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/earchive/{ettn}/pdf/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEArchiveHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/earchive/{ettn}/html/{standardXslt}", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<byte[]>> GetEArchiveUblAsync(Guid ettn, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v2/earchive/{ettn}/ubl", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetEArchiveWithNullLocalReferencesAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var url = $"/v2/earchive/withnulllocalreferences?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}";
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> RetryEArchiveMailAsync(Guid id, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, $"/v1/earchive/retryinvoicemail/{id}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> RetryEArchiveMailToAddressesAsync(EArchiveMailRetryRequest request, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Post, "/v1/earchive/retryinvoicemail", ct);
            req.Content = JsonContent.Create(request);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> CancelEArchiveAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Put, "/v1/earchive/cancelinvoice", ct);
            req.Content = JsonContent.Create(ids);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-İrsaliye ----------
        public async Task<HttpResult<object>> SendDespatchUblAsync(Invoice inv, string? targetAlias = null, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
        {
            var body = MapToDespatchUbl(inv, targetAlias);
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Post, "/v1/outboxdespatch", ct);
            req.Content = JsonContent.Create(body);
            var res = await SendAsync<object>(_http, req, ct);
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-İrsaliye Gönderim (UBL)", ct);
            return res;
        }

        public async Task<HttpResult<object>> UpdateDespatchStatusListAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Put, "/v2/outboxdespatch/updatestatuslist", ct);
            req.Content = JsonContent.Create(new { ids, status });
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> GetDespatchInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default)
        {
            var url =
                $"/v1/inboxdespatch/list?startDate={start:yyyy-MM-dd HH:mm:ss}&endDate={end:yyyy-MM-dd HH:mm:ss}&pageIndex={pageIndex}&pageSize={pageSize}&isDesc={isDesc}";
            using var req = await CreateReqAsync(_opt.EIrsaliyeBaseUrl, HttpMethod.Get, url, ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- e-Defter ----------
        public async Task<HttpResult<object>> GetEDefterPeriodListAsync(int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EDefterBaseUrl, HttpMethod.Get, $"/v1/period/getperiodlist?pageIndex={pageIndex}&pageSize={pageSize}&isDesc={isDesc}", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        public async Task<HttpResult<object>> PostEDefterAsync(PostEDefterRequest model, Stream zipStream, string fileName,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default)
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
            if (consumeKontor && res.Ok)
                await ConsumeBalanceAsync(kontorVkn ?? _opt.TestSenderVkn, 1, "e-Defter Yükleme", ct);
            return res;
        }

        public async Task<HttpResult<object>> SendLetterPatentsToGibAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EDefterBaseUrl, HttpMethod.Get, "/v1/sendletterpattentstogib", ct);
            return await SendAsync<object>(_http, req, ct);
        }

        // ---------- GIB User ----------
        public async Task<HttpResult<byte[]>> GetGibUserRecipientZipAsync(CancellationToken ct = default)
        {
            using var req = await CreateReqAsync(_opt.EFaturaBaseUrl, HttpMethod.Get, "/v2/gibuser/recipient/zip", ct);
            return await SendAsync<byte[]>(_http, req, ct);
        }

        // -------- Yardımcı: faturanın ayına göre e-Defter request --------
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
    }
}
