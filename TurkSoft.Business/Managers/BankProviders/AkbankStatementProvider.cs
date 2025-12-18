using AkbankSrv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class AkbankStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.Akbank;
        public string BankCode => "AKB";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            var list = new List<BNKHAR>();

            // Endpoint’i request.Link ile override etmek istersen:
            ServiceSoapClient client = string.IsNullOrWhiteSpace(request.Link)
                ? new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap)
                : new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap, request.Link);

            try
            {
                // Eğer Akbank HTTP Basic auth istiyorsa (emin değiliz ama stabil dursun):
                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    if (client.Endpoint.Binding is BasicHttpBinding b)
                        b.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

                    client.ClientCredentials.UserName.UserName = request.Username;
                    client.ClientCredentials.UserName.Password = request.Password ?? "";
                }

                for (var day = request.BeginDate.Date; day <= request.EndDate.Date; day = day.AddDays(1))
                {
                    var basTarih = day.ToString("yyyyMMdd") + "000000000000";
                    var bitTarih = day.ToString("yyyyMMdd") + "230000000000";

                    XmlNode xml = await client.GetExtreWithParamsAsync(
                        urf: "",
                        hesapNo: request.AccountNumber,
                        dovizKodu: "",
                        subeKodu: "",
                        baslangicTarihi: basTarih,
                        bitisTarihi: bitTarih
                    ).ConfigureAwait(false);

                    if (xml == null) continue;

                    var doc = new XmlDocument();
                    doc.LoadXml(xml.OuterXml);

                    var detayList = doc.GetElementsByTagName("Detay");
                    var hesapList = doc.GetElementsByTagName("Hesap");

                    foreach (XmlNode node in detayList)
                    {
                        string pTim = GetChild(node, 0);
                        string pTim2 = GetChild(node, 1);

                        var row = new BNKHAR
                        {
                            BNKCODE = BankCode,
                            HESAPNO = request.AccountNumber,

                            PROCESSTIMESTR = pTim,
                            PROCESSTIMESTR2 = pTim2,
                            PROCESSTIME = ParseYmd(pTim),
                            PROCESSTIME2 = ParseYmd(pTim2),

                            PROCESSREFNO = GetChild(node, 12),
                            PROCESSIBAN = GetChild(node, 24),
                            PROCESSVKN = GetChild(node, 21),
                            PROCESSAMAOUNT = GetChild(node, 4),
                            PROCESSBALANCE = GetChild(node, 5),
                            PROCESSDESC = GetChild(node, 6),
                            PROCESSDESC2 = GetChild(node, 25),
                            PROCESSDESC3 = GetChild(node, 26),
                            PROCESSDEBORCRED = GetChild(node, 8) == "+" ? "A" : "B",
                            PROCESSTYPECODE = GetChild(node, 10),
                            PROCESSTYPECODEMT940 = GetChild(node, 11),
                            Durum = 0
                        };

                        foreach (XmlNode h in hesapList)
                        {
                            row.URF = GetChild(h, 3);
                            row.SUBECODE = GetChild(h, 4);
                            row.FRMIBAN = GetChild(h, 7);
                            row.PROCESSID = GetChild(h, 0);
                        }

                        row.PROCESSID ??= row.PROCESSREFNO;
                        list.Add(row);
                    }
                }

                return list;
            }
            finally
            {
                client.SafeClose();
            }
        }

        private static string GetChild(XmlNode node, int index)
            => node.ChildNodes.Count > index ? node.ChildNodes[index].InnerText : "";

        private static DateTime? ParseYmd(string ymd)
        {
            if (string.IsNullOrWhiteSpace(ymd) || ymd.Length < 8) return null;
            if (!int.TryParse(ymd.Substring(0, 4), out var y)) return null;
            if (!int.TryParse(ymd.Substring(4, 2), out var m)) return null;
            if (!int.TryParse(ymd.Substring(6, 2), out var d)) return null;
            return new DateTime(y, m, d);
        }
    }
}
