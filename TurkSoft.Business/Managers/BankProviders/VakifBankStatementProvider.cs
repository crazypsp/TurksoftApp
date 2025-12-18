using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;
using VakifSrv = VakıfSrv;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class VakifBankStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.Vakifbank;
        public string BankCode => "VAK";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            var list = new List<BNKHAR>();

            // Vakıf için MusteriNo gerekiyor (sende nerede tutuyorsan oradan ver)
            // Örn: request.Username = KurumKullanici, request.Password = Sifre, request.CustomerNo = MusteriNo gibi.
            var musteriNo = request.GetExtraRequired("MusteriNo"); // eğer böyle bir sistemin varsa
                                                                   // yoksa üst satırı kaldırıp: var musteriNo = request.Username; gibi ayarla.

            var client = string.IsNullOrWhiteSpace(request.Link)
                ? new VakifSrv.SOnlineEkstreServisClient(VakifSrv.SOnlineEkstreServisClient.EndpointConfiguration.MetadataExchangeHttpsBinding_ISOnlineEkstreServis)
                : new VakifSrv.SOnlineEkstreServisClient(VakifSrv.SOnlineEkstreServisClient.EndpointConfiguration.MetadataExchangeHttpsBinding_ISOnlineEkstreServis, request.Link);

            try
            {
                var sorgu = new VakifSrv.DtoEkstreSorgu
                {
                    HesapNo = request.AccountNumber,
                    MusteriNo = musteriNo,
                    KurumKullanici = request.Username,
                    Sifre = request.Password ?? "",
                    SorguBaslangicTarihi = request.BeginDate.ToString("yyyy-MM-dd 00:00"),
                    SorguBitisTarihi = request.EndDate.ToString("yyyy-MM-dd 23:59"),
                };

                var resp = await client.GetirHareketAsync(sorgu).ConfigureAwait(false);
                if (resp == null) return list;

                // Başarılı mı kontrolü (sende beklediğin kod neyse)
                if (!string.Equals(resp.IslemKodu, "VBB0001", StringComparison.OrdinalIgnoreCase))
                    return list;

                foreach (var hesap in resp.Hesaplar ?? Array.Empty<VakifSrv.DtoEkstreHesap>())
                {
                    foreach (var h in hesap.Hareketler ?? Array.Empty<VakifSrv.DtoEkstreHareket>())
                    {
                        list.Add(new BNKHAR
                        {
                            BNKCODE = BankCode,
                            HESAPNO = hesap.HesapNo,
                            FRMIBAN = hesap.HesapNoIban,
                            FRMVKN = hesap.VergiKimlikNumarasi,

                            PROCESSID = h.Id.ToString(),
                            PROCESSTIMESTR = h.IslemTarihi,
                            PROCESSTIMESTR2 = h.IslemTarihi,
                            PROCESSREFNO = h.IslemNo,
                            PROCESSDESC = h.Aciklama,
                            PROCESSDESC2 = h.IslemAdi,
                            PROCESSDEBORCRED = h.BorcAlacak,
                            PROCESSTYPECODEMT940 = h.IslemKodu,
                            PROCESSAMAOUNT = h.Tutar.ToString(),
                            PROCESSBALANCE = h.IslemSonrasıBakiye.ToString(), // Türkçe “ı” olan property
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
    }
}
