using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    /// <summary>
    /// QNB Finansbank - MaestroCoreEkstre / TeknikBaglantiService SOAP entegrasyonu
    /// WSDL: https://fbmaestro.qnb.com.tr/MaestroCoreEkstre/services/TeknikBaglantiService?wsdl
    /// </summary>
    public sealed class QnbMaestroStatementProvider : IBankStatementProvider
    {
        // WSDL içinde SOAP11 address "http://fbmaestro.qnb.com.tr:9086/..." olarak geliyor.
        private const string DefaultSoap11Endpoint =
            "http://fbmaestro.qnb.com.tr:9086/MaestroCoreEkstre/services/TeknikBaglantiService.TeknikBaglantiServiceHttpSoap11Endpoint/";

        public int BankId => BankIds.QnbFinans;
        public string BankCode => "QNB";

        private readonly IHttpClientFactory _httpFactory;

        public QnbMaestroStatementProvider(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("QNB için Username zorunlu.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("QNB için Password zorunlu.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("QNB için AccountNumber zorunlu.");

            var endpoint = string.IsNullOrWhiteSpace(request.Link) ? DefaultSoap11Endpoint : request.Link.Trim();

            // iban opsiyonel (istersen Extras içine "iban" koyarsın)
            var iban = request.GetExtra("iban"); // yoksa null

            var soapXml = BuildSoapEnvelope(
                userName: request.Username,
                password: request.Password,
                accountNo: request.AccountNumber,
                start: request.BeginDate,
                end: request.EndDate,
                iban: iban
            );

            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(90);

            using var msg = new HttpRequestMessage(HttpMethod.Post, endpoint);
            msg.Headers.TryAddWithoutValidation("SOAPAction", "\"urn:getTransactionInfo\"");
            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            msg.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");

            using var resp = await http.SendAsync(msg, ct).ConfigureAwait(false);
            var respText = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"QNB servis HTTP {(int)resp.StatusCode}: {SafePreview(respText)}");

            return ParseResponseToBnkhar(respText, request);
        }

        private static string BuildSoapEnvelope(
            string userName,
            string password,
            string accountNo,
            DateTime start,
            DateTime end,
            string? iban)
        {
            // WSDL’de kompleks tiplerin elementleri elementFormDefault="unqualified".
            // Bu yüzden iç alanları (userName/password/accountNo/...) prefixsiz göndermek daha uyumludur.
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns = "http://teknikbaglantiekstre.genericekstrenew2.driver.maestro.ibtech.com";

            var body =
                new XElement(ns + "getTransactionInfo",
                    new XElement("transactionInfo",
                        new XElement("password", password),
                        new XElement("transactionInfoInputType",
                            new XElement("accountNo", accountNo),
                            new XElement("startDate", start.ToString("o", CultureInfo.InvariantCulture)),
                            new XElement("endDate", end.ToString("o", CultureInfo.InvariantCulture)),
                            string.IsNullOrWhiteSpace(iban) ? null : new XElement("iban", iban)
                        ),
                        new XElement("userName", userName)
                    )
                );

            var doc =
                new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(soap + "Envelope",
                        new XAttribute(XNamespace.Xmlns + "soapenv", soap),
                        new XAttribute(XNamespace.Xmlns + "ns", ns),
                        new XElement(soap + "Header"),
                        new XElement(soap + "Body", body)
                    )
                );

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private static IReadOnlyList<BNKHAR> ParseResponseToBnkhar(string xml, BankStatementRequest request)
        {
            var doc = XDocument.Parse(xml);

            // SOAP Fault kontrolü
            var fault = doc.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("Fault", StringComparison.OrdinalIgnoreCase));
            if (fault != null)
            {
                var faultString =
                    fault.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("faultstring", StringComparison.OrdinalIgnoreCase))?.Value
                    ?? fault.Value;
                throw new Exception($"QNB SOAP Fault: {SafePreview(faultString)}");
            }

            // <return> içinde hata alanları var
            var ret = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "return");
            if (ret == null) return Array.Empty<BNKHAR>();

            var errorCode = GetVal(ret, "errorCode");
            var errorDesc = GetVal(ret, "errorDescription");

            // Bazı servisler "0" veya boş döner, bazıları null; burada dolu ve 0 değilse hata sayalım
            if (!string.IsNullOrWhiteSpace(errorCode) && errorCode != "0")
                throw new Exception($"QNB hata. Code={errorCode} Desc={errorDesc}");

            var list = new List<BNKHAR>();

            // accountInfos (maxOccurs unbounded)
            var accountInfos = ret.Descendants().Where(x => x.Name.LocalName == "accountInfos");
            foreach (var acc in accountInfos)
            {
                var accNo = GetVal(acc, "accountNo");
                var customerNo = GetVal(acc, "customerNo");
                var branchCode = GetVal(acc, "branchCode");
                var accCurrency = GetVal(acc, "accountCurrencyCode");
                var iban = GetVal(acc, "iban");

                // transactions (maxOccurs unbounded)
                var transactions = acc.Descendants().Where(x => x.Name.LocalName == "transactions");
                foreach (var trx in transactions)
                {
                    var trxId = GetVal(trx, "transactionId");
                    var order = GetVal(trx, "statementTransactionOrder");
                    var pid = !string.IsNullOrWhiteSpace(trxId) ? trxId : order;

                    var trxDateStr = GetVal(trx, "transactionDate");
                    var dt = ParseDate(trxDateStr);

                    var debitCredit = GetVal(trx, "debitOrCreditCode");
                    var dcMapped = MapDebitCredit(debitCredit);

                    var amount = GetVal(trx, "transactionAmount");
                    var balance = GetVal(trx, "transactionBalance");
                    var desc = GetVal(trx, "transactionDescription");

                    var opponentIban = GetVal(trx, "opponentIBAN");
                    var opponentTax = GetVal(trx, "opponentTAXNoPIDNo");

                    var refNo =
                        FirstNonEmpty(
                            GetVal(trx, "eftInquiryNumber"),
                            GetVal(trx, "ddReferenceNumber"),
                            GetVal(trx, "ddFirmReferenceNumber"),
                            GetVal(trx, "productOperationRefNo"),
                            pid
                        );

                    list.Add(new BNKHAR
                    {
                        BNKCODE = "QNB",
                        HESAPNO = string.IsNullOrWhiteSpace(accNo) ? request.AccountNumber : accNo,
                        URF = customerNo,
                        SUBECODE = branchCode,
                        CURRENCYCODE = FirstNonEmpty(GetVal(trx, "currencyCode"), accCurrency),

                        PROCESSID = pid,
                        PROCESSTIMESTR = trxDateStr,
                        PROCESSTIMESTR2 = trxDateStr,
                        PROCESSTIME = dt,
                        PROCESSTIME2 = dt,

                        PROCESSAMAOUNT = amount,
                        PROCESSBALANCE = balance,
                        PROCESSDESC = desc,
                        PROCESSDEBORCRED = dcMapped,

                        PROCESSREFNO = refNo,
                        PROCESSIBAN = opponentIban,
                        PROCESSVKN = opponentTax,

                        FRMIBAN = iban,
                        Durum = 0
                    });
                }
            }

            return list;
        }

        private static string GetVal(XElement parent, string localName)
            => parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value?.Trim() ?? "";

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                return dt;

            if (DateTime.TryParse(s, out dt))
                return dt;

            return null;
        }

        private static string MapDebitCredit(string code)
        {
            // Mevcut sisteminde "A" alacak, "B" borç gibi kullanıyordun.
            // QNB’de genelde "C/D" veya benzeri gelebilir.
            if (string.IsNullOrWhiteSpace(code)) return "";

            var c = code.Trim().ToUpperInvariant();
            if (c.Contains("C") || c == "+") return "A"; // Credit/Alacak
            if (c.Contains("D") || c == "-") return "B"; // Debit/Borç

            return code; // bilinmiyorsa olduğu gibi
        }

        private static string FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? "";

        private static string SafePreview(string? s)
        {
            s ??= "";
            s = s.Trim();
            return s.Length <= 600 ? s : s[..600];
        }
    }
}
