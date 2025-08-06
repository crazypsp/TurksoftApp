using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    /// <summary>
    /// Banka ekstre analizine ilişkin servis işlemleri.
    /// İş katmanındaki IBankaEkstreAnalyzerBusiness arayüzünü kullanır.
    /// </summary>
    public class BankaEkstreAnalyzerManagerSrv : IBankaEkstreAnalyzerService
    {
        private readonly IBankaEkstreAnalyzerBusiness _bankaEkstreAnalyzerBusiness;

        public BankaEkstreAnalyzerManagerSrv(IBankaEkstreAnalyzerBusiness bankaEkstreAnalyzerBusiness)
        {
            _bankaEkstreAnalyzerBusiness = bankaEkstreAnalyzerBusiness;
        }

        public async Task<IDataResult<List<LucaFisRow>>> HesapKodlariIleEsleAsync(
            List<BankaHareket> hareketler,
            List<AccountingCode> hesapKodlari,
            Dictionary<string, string> keywordMap,
            string bankaHesapKodu)
        {
            return await _bankaEkstreAnalyzerBusiness.HareketleriFisSatirlarinaDonusturAsync(hareketler, hesapKodlari, keywordMap, bankaHesapKodu);
        }
    }
}
