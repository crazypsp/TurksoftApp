using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;

namespace TurkSoft.Business.Interface
{
    public interface IBankaEkstreAnalyzerBusiness
    {
        /// <summary>
        /// Banka hareketlerinin açıklamalarını analiz ederek en uygun hesap kodunu atar.
        /// </summary>
        /// <param name="hareketler">Eşleme yapılacak banka hareketleri</param>
        /// <param name="hesapKodlari">Kullanılabilir tüm hesap kodları</param>
        /// <param name="keywordMap">
        /// API’den gelen anahtar‐kelime → hesap kodu sözlüğü 
        /// (örn. { "maaş" → "700.01.001", "kira" → "630.01.001", … })
        /// </param>
        /// <param name="bankaHesapKodu">Banka hesabının kodu (örneğin GARANTİ BANKASI kodu)</param>
        /// <returns>Oluşturulmuş LucaFisRow listesi</returns>
        Task<IDataResult<List<LucaFisRow>>> HareketleriFisSatirlarinaDonusturAsync(
            List<BankaHareket> hareketler,
        List<AccountingCode> hesapKodlari,
        Dictionary<string, string> keywordMap,
        string bankaHesapKodu
        );
    }
}
