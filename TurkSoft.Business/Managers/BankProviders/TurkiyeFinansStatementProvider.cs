using System.Globalization;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

using TurkiyeFinansSrv; // svcutil namespace

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class TurkiyeFinansStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.TurkiyeFinans;
        public string BankCode => "TFN";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Türkiye Finans için Username zorunlu.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Türkiye Finans için Password zorunlu.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Türkiye Finans için AccountNumber zorunlu.");

            // Tarih formatı bankaya göre değişebiliyor; proxy string bekliyor.
            // Default: yyyy-MM-dd
            var dateFormat = request.GetExtra("dateFormat") ?? "yyyy-MM-dd";
            var beginStr = request.BeginDate.ToString(dateFormat, CultureInfo.InvariantCulture);
            var endStr = request.EndDate.ToString(dateFormat, CultureInfo.InvariantCulture);

            // RefNo opsiyonel
            var refNo = request.GetExtra("refNo") ?? "";

            // Client (Link override edilebilir ama WSDL değil endpoint vermelisin)
            var endpoint = string.IsNullOrWhiteSpace(request.Link)
                ? null
                : request.Link;

            var client = endpoint is null
                ? new BankExtractIntegrationServiceClient(BankExtractIntegrationServiceClient.EndpointConfiguration.IntegrationBinding_IBankExtractIntegrationService)
                : new BankExtractIntegrationServiceClient(BankExtractIntegrationServiceClient.EndpointConfiguration.IntegrationBinding_IBankExtractIntegrationService, endpoint);

            try
            {
                // QueryExtractAsync(string KullaniciKod, string Sifre, string HesapNo, string BaslangicTarihi, string BitisTarihi, string RefNo)
                var resp = await client.QueryExtractAsync(
                    request.Username,
                    request.Password,
                    request.AccountNumber,
                    beginStr,
                    endStr,
                    refNo
                );

                var mv = resp?.Hareketler;
                if (mv == null)
                    throw new Exception("Türkiye Finans: Boş yanıt döndü.");

                if (!string.IsNullOrWhiteSpace(mv.hataKodu) && mv.hataKodu != "0")
                    throw new Exception($"Türkiye Finans: {mv.hataKodu} - {mv.hataAciklama}");

                var acc = mv.Hesap;
                if (acc == null)
                    return Array.Empty<BNKHAR>();

                var list = new List<BNKHAR>();
                var acts = acc.Aktiviteler ?? Array.Empty<Hareket>();

                foreach (var h in acts)
                {
                    var pid = FirstNonEmpty(h.UniqueReference, h.RefNo, h.FisNo) ?? Guid.NewGuid().ToString("N");

                    // Tarih+saat parse (esnek)
                    var dt = ParseDateTime(h.Tarih, h.IslemSaati);
                    var valDt = ParseDateTime(h.Valor, null);

                    // Borç/Alacak tahmini (tutar işaretinden)
                    var debCred = GuessDebitCredit(h.Tutar);

                    list.Add(new BNKHAR
                    {
                        BNKCODE = BankCode,
                        HESAPNO = acc.HesapNo ?? request.AccountNumber,

                        URF = acc.MusteriNo ?? request.GetExtra("musteriNo") ?? "",
                        SUBECODE = acc.SubeKodu ?? "",
                        CURRENCYCODE = acc.ParaKod ?? "",

                        // Bizim hesap IBAN
                        FRMIBAN = acc.IBAN ?? "",

                        // Hareket içindeki IBAN çoğu entegrasyonda karşı IBAN olabiliyor
                        PROCESSIBAN = h.Iban ?? "",

                        PROCESSID = pid,
                        PROCESSREFNO = h.RefNo ?? "",

                        PROCESSTIMESTR = $"{h.Tarih} {h.IslemSaati}".Trim(),
                        PROCESSTIMESTR2 = h.Valor ?? h.Tarih ?? "",
                        PROCESSTIME = dt,
                        PROCESSTIME2 = valDt ?? dt,

                        PROCESSAMAOUNT = h.Tutar ?? "0",
                        PROCESSBALANCE = h.IslemSonuBakiye ?? acc.Bakiye ?? "0",

                        PROCESSDESC = h.Aciklama1 ?? "",
                        PROCESSDESC2 = h.Aciklama2 ?? "",
                        PROCESSDESC3 = h.EftDescription ?? "",

                        PROCESSVKN = h.Vkn ?? "",
                        PROCESSDEBORCRED = debCred,              // "A" / "B"
                        PROCESSTYPECODE = h.ProgramKod ?? "",
                        PROCESSTYPECODEMT940 = null,

                        Durum = 0
                    });
                }

                return list;
            }
            finally
            {
                client.SafeClose(); // senin WcfCloseExtensions.SafeClose()
            }
        }

        private static string? FirstNonEmpty(params string?[] vals)
            => vals.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        private static string GuessDebitCredit(string? amount)
        {
            if (string.IsNullOrWhiteSpace(amount)) return "";

            var a = amount.Trim();

            // -100,00 gibi ise Borç
            if (a.StartsWith("-", StringComparison.OrdinalIgnoreCase)) return "B";

            // bazen (100,00) negatif anlamında kullanılır
            if (a.StartsWith("(", StringComparison.OrdinalIgnoreCase) && a.EndsWith(")")) return "B";

            // aksi halde Alacak varsay
            return "A";
        }

        private static DateTime? ParseDateTime(string? date, string? time)
        {
            if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(time)) return null;

            var raw = (date ?? "").Trim();
            var t = (time ?? "").Trim();

            // Birleştir
            var s = string.IsNullOrWhiteSpace(t) ? raw : $"{raw} {t}";

            // Olası formatlar
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd",

                "dd.MM.yyyy HH:mm:ss",
                "dd.MM.yyyy HH:mm",
                "dd.MM.yyyy",

                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy",

                "yyyyMMddHHmmss",
                "yyyyMMdd"
            };

            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var dt))
                return dt;

            // son çare
            if (DateTime.TryParse(s, CultureInfo.GetCultureInfo("tr-TR"),
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out dt))
                return dt;

            return null;
        }
    }
}
