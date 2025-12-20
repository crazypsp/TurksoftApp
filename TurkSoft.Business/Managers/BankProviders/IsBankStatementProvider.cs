using System.Net;
using System.Text;
using System.Xml;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class IsBankStatementProvider : IBankStatementProvider
    {
        static IsBankStatementProvider()
        {
            // Host tarafı unutsa bile burada garanti
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public int BankId => BankIds.IsBankasi;
        public string BankCode => "ISB";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Link) || string.IsNullOrWhiteSpace(request.TLink))
                throw new ArgumentException("İşBank için Link ve TLink zorunlu.");

            var cookies = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                AllowAutoRedirect = true,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using var http = new HttpClient(handler);
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36"
            );

            // 1) cookie al
            _ = await http.GetAsync(request.Link, ct).ConfigureAwait(false);

            // 2) login
            var post = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["uid"] = request.Username,
                ["pwd"] = request.Password
            });

            var resp = await http.PostAsync(request.TLink, post, ct).ConfigureAwait(false);
            var bytes = await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);

            // 3) KESİN decoding: ISO-8859-9 (28599) -> fallback 1254
            string content;
            try
            {
                content = Encoding.GetEncoding(28599).GetString(bytes); // ISO-8859-9
            }
            catch
            {
                content = Encoding.GetEncoding(1254).GetString(bytes);  // Windows-1254
            }

            content = (content ?? "").Trim();
            if (!content.Contains("<Hesap", StringComparison.OrdinalIgnoreCase))
            {
                var preview = content.Length > 500 ? content[..500] : content;
                throw new Exception($"İşBank XML yerine beklenmeyen içerik döndü (ilk 500): {preview}");
            }

            var xml = new XmlDocument();
            xml.LoadXml(content);

            var list = new List<BNKHAR>();
            var hesapNodes = xml.GetElementsByTagName("Hesap");

            foreach (XmlNode hesapNode in hesapNodes)
            {
                var doc = new XmlDocument();
                doc.LoadXml(hesapNode.OuterXml.Trim());

                var tanim = doc.SelectSingleNode("Hesap/Tanimlamalar");
                var hareketler = doc.SelectNodes("Hesap/Hareketler/Hareket");
                if (tanim == null || hareketler == null) continue;

                foreach (XmlNode h in hareketler)
                {
                    var pid = h.SelectSingleNode(".//HareketSirano")?.InnerText ?? "";
                    var ts = h.SelectSingleNode(".//timeStamp")?.InnerText ?? "";
                    DateTime? dt = DateTime.TryParse(ts, out var dtx) ? dtx : null;

                    list.Add(new BNKHAR
                    {
                        BNKCODE = BankCode,
                        HESAPNO = tanim.SelectSingleNode(".//HesapNo")?.InnerText ?? request.AccountNumber,
                        URF = tanim.SelectSingleNode(".//MusteriNo")?.InnerText ?? "",
                        SUBECODE = tanim.SelectSingleNode(".//SubeKodu")?.InnerText ?? "",
                        CURRENCYCODE = h.SelectSingleNode(".//ParaBirimi")?.InnerText,

                        PROCESSID = pid,
                        PROCESSTIMESTR = ts,
                        PROCESSTIMESTR2 = tanim.SelectSingleNode(".//SonHareketTarihi")?.InnerText ?? ts,
                        PROCESSTIME = dt,
                        PROCESSTIME2 = dt,

                        PROCESSVKN = h.SelectSingleNode(".//KarsiHesapVKN")?.InnerText ?? "",
                        PROCESSAMAOUNT = h.SelectSingleNode(".//Miktar")?.InnerText ?? "0",
                        PROCESSBALANCE = h.SelectSingleNode(".//Bakiye")?.InnerText ?? "0",
                        PROCESSDESC = h.SelectSingleNode(".//Aciklama")?.InnerText ?? "",
                        PROCESSDESC2 = h.SelectSingleNode(".//Kaynak")?.InnerText ?? "",
                        PROCESSDEBORCRED = h.SelectSingleNode(".//borcAlacak")?.InnerText ?? "",
                        PROCESSTYPECODE = h.SelectSingleNode(".//IslemTuru")?.InnerText ?? "",
                        PROCESSTYPECODEMT940 = h.SelectSingleNode(".//IslemTod")?.InnerText,
                        PROCESSREFNO = h.SelectSingleNode(".//DekontNo")?.InnerText ?? "",
                        Durum = 0
                    });
                }
            }

            return list;
        }
    }
}
