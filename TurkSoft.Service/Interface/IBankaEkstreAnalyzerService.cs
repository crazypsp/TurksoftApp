using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;

namespace TurkSoft.Service.Interface
{
    /// <summary>
    /// Banka ekstre analizine ilişkin servis işlevlerini tanımlar.
    /// </summary>
    public interface IBankaEkstreAnalyzerService
    {
        /// <summary>
        /// Banka hareketlerinden çift kayıt oluşturarak LucaFisRow listesi döndürür.
        /// </summary>
        Task<IDataResult<List<LucaFisRow>>> HesapKodlariIleEsleAsync(
            List<BankaHareket> hareketler,
            List<AccountingCode> hesapKodlari,
            Dictionary<string, string> keywordMap,
            string bankaHesapKodu
        );
    }
}
