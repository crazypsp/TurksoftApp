using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class IsBankStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.IsBankasi;
        public string BankCode => "ISB";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Link) || string.IsNullOrWhiteSpace(request.TLink))
                throw new ArgumentException("İşBank için Link ve TLink zorunlu.");

            var cookies = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = true, UseCookies = true };

            using var http = new HttpClient(handler);

            _ = await http.GetAsync(request.Link, ct);

            var post = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["uid"] = request.Username,
                ["pwd"] = request.Password
            });

            var resp = await http.PostAsync(request.TLink, post, ct);
            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
            var content = Encoding.GetEncoding("iso-8859-9").GetString(bytes);

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
