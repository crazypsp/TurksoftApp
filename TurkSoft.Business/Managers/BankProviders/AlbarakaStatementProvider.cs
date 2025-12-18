using AlbarakaTurkSrv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class AlbarakaStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.AlbarakaTurk;
        public string BankCode => "ALB";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            var list = new List<BNKHAR>();

            HesapBilgileriServiceSoapClient client = string.IsNullOrWhiteSpace(request.Link)
                ? new HesapBilgileriServiceSoapClient(HesapBilgileriServiceSoapClient.EndpointConfiguration.HesapBilgileriServiceSoap)
                : new HesapBilgileriServiceSoapClient(HesapBilgileriServiceSoapClient.EndpointConfiguration.HesapBilgileriServiceSoap, request.Link);

            try
            {
                var input = new hesapHareketleriInput
                {
                    musteriNo = request.Username,              // sizde musteriNo = username gibi kullanılıyor
                    hesapNo = request.AccountNumber,
                    basTarih = request.BeginDate.ToString("yyyyMMdd"),
                    sonTarih = request.EndDate.ToString("yyyyMMdd")
                };

                var resp = await client.getHesapHareketleriAsync(
                    pId: request.Username,
                    pIdPass: request.Password ?? "",
                    pParams: input
                ).ConfigureAwait(false);

                var data = resp?.responseData;
                var hesaplar = data?.hesapHareketleri ?? Array.Empty<hesapHareketleriHesap>();

                foreach (var hesap in hesaplar)
                {
                    var hareketler = hesap.hesapHareket ?? Array.Empty<hesapHareket>();

                    foreach (var trn in hareketler)
                    {
                        var dt = TryParseYmd(trn.tarih);

                        list.Add(new BNKHAR
                        {
                            BNKCODE = BankCode,
                            HESAPNO = hesap.hesapNo,
                            FRMIBAN = hesap.hesapIban,
                            FRMVKN = hesap.hesapTcknVkn,

                            PROCESSID = trn.fisNo,
                            PROCESSTIMESTR = trn.tarih,
                            PROCESSTIMESTR2 = trn.saat,
                            PROCESSTIME = dt,
                            PROCESSTIME2 = dt,

                            PROCESSREFNO = trn.MuhRefNo,
                            PROCESSAMAOUNT = trn.islemTutari,
                            PROCESSBALANCE = trn.islemsonrasibakiye, // varsa bunu tercih et
                            PROCESSDESC = trn.aciklama,
                            PROCESSDESC3 = trn.karsiHesapIban,
                            PROCESSDESC4 = trn.karsiHesapTcknVkn,
                            PROCESSDEBORCRED = trn.borcAlacak,
                            PROCESSTYPECODE = trn.Code,
                            Durum = 0
                        });
                    }
                }

                return list;
            }
            finally
            {
                client.SafeClose();
            }
        }

        private static DateTime? TryParseYmd(string yyyymmdd)
        {
            if (string.IsNullOrWhiteSpace(yyyymmdd) || yyyymmdd.Length < 8) return null;
            if (!int.TryParse(yyyymmdd[..4], out var y)) return null;
            if (!int.TryParse(yyyymmdd.Substring(4, 2), out var m)) return null;
            if (!int.TryParse(yyyymmdd.Substring(6, 2), out var d)) return null;
            return new DateTime(y, m, d);
        }
    }
}
