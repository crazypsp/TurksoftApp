using System;
using System.Collections.Generic;
using System.Linq;
using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Class;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;

public class BankaEkstreAnalyzerManager : IBankaEkstreAnalyzerBusiness
{
    // Benzerlik skoru için alt eşik (0 ile 1 arasında)
    private const double SimilarityThreshold = 0.3;

    /// <summary>
    /// Banka hareketlerini analiz ederek her hareket için karşı hesap kodunu belirler 
    /// ve banka + karşı hesap olacak şekilde LucaFisRow fiş satırlarını oluşturur.
    /// </summary>
    /// <param name="hareketler">Analiz edilecek banka hareket listesi</param>
    /// <param name="hesapKodlari">Tüm kullanılabilir hesap kodları ve adları listesi</param>
    /// <param name="keywordMap">Anahtar kelime -> Hesap kodu eşleme sözlüğü</param>
    /// <param name="bankaHesapKodu">İşlem yapılan banka hesabının kodu (örn. 102.03.004)</param>
    /// <returns>Fiş satırlarını içeren sonuç nesnesi (IDataResult)</returns>
    public Task<IDataResult<List<LucaFisRow>>> HareketleriFisSatirlarinaDonusturAsync(
        List<BankaHareket> hareketler,
        List<AccountingCode> hesapKodlari,
        Dictionary<string, string> keywordMap,
        string bankaHesapKodu)
    {
        var fisSatirlari = new List<LucaFisRow>();

        foreach (var hareket in hareketler)
        {
            string aciklama = hareket.Aciklama ?? string.Empty;
            string karsiHesapKodu = null;

            // 1. Anahtar kelime ile eşleştirme
            var keywordMatch = keywordMap.FirstOrDefault(kvp =>
                aciklama.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(keywordMatch.Value))
            {
                karsiHesapKodu = keywordMatch.Value;
            }
            else
            {
                // 2. Hesap adı ile doğrudan eşleştirme
                var directMatch = hesapKodlari.FirstOrDefault(h =>
                    !string.IsNullOrEmpty(h.Name) &&
                    aciklama.IndexOf(h.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                if (directMatch != null)
                {
                    karsiHesapKodu = directMatch.Code;
                }
                else
                {
                    // 3. Levenshtein benzerliği ile en yakın hesap eşleştirmesi
                    var bestMatch = hesapKodlari.Select(h => new
                    {
                        Code = h.Code,
                        Name = h.Name,
                        Score = GetSimilarity(aciklama, h.Name)
                    })
                                        .OrderByDescending(x => x.Score)
                                        .FirstOrDefault();
                    if (bestMatch != null && bestMatch.Score >= SimilarityThreshold)
                    {
                        karsiHesapKodu = bestMatch.Code;
                    }
                }
            }

            // Eğer karşı hesap kodu bulunamadıysa bu hareketi atla (eşleşme yok)
            if (string.IsNullOrEmpty(karsiHesapKodu))
                continue;

            // Tutar ve borç/alacak belirleme
            decimal tutar = hareket.Tutar;
            decimal absTutar = Math.Abs(tutar);

            if (tutar > 0)
            {
                // Para girişi: Banka hesabı borç, karşı hesap alacak
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = hareket.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = bankaHesapKodu,
                    Aciklama = aciklama,
                    Borc = absTutar,
                    Alacak = 0m
                });
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = hareket.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = karsiHesapKodu,
                    Aciklama = aciklama,
                    Borc = 0m,
                    Alacak = absTutar
                });
            }
            else if (tutar < 0)
            {
                // Para çıkışı: Banka hesabı alacak, karşı hesap borç
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = hareket.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = karsiHesapKodu,
                    Aciklama = aciklama,
                    Borc = absTutar,
                    Alacak = 0m
                });
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = hareket.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = bankaHesapKodu,
                    Aciklama = aciklama,
                    Borc = 0m,
                    Alacak = absTutar
                });
            }
            else
            {
                // Tutar 0 ise fiş satırı oluşturulmasına gerek yok (atla)
                continue;
            }
        }

        // Sonuç nesnesini oluştur (başarılı)
        return Task.FromResult<IDataResult<List<LucaFisRow>>>(
            new DataResult<List<LucaFisRow>>(fisSatirlari, true, "Fiş satırları başarıyla oluşturuldu."));
    }

    /// <summary>
    /// İki metin arasında normalize edilmiş Levenshtein benzerlik skorunu hesaplar (0.0 - 1.0 arası).
    /// </summary>
    private static double GetSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            return 1.0;
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        // Levenshtein mesafesini kullanarak benzerlik oranını hesapla
        int dist = LevenshteinDistance(s1, s2);
        int maxLen = Math.Max(s1.Length, s2.Length);
        return maxLen == 0 ? 1.0 : (1.0 - (double)dist / maxLen);
    }

    /// <summary>
    /// Levenshtein mesafesini hesaplar (iki string arasındaki düzenleme uzaklığı).
    /// </summary>
    private static int LevenshteinDistance(string a, string b)
    {
        int n = a.Length;
        int m = b.Length;
        var dp = new int[n + 1, m + 1];

        // Başlangıç değerleri
        for (int i = 0; i <= n; i++) dp[i, 0] = i;
        for (int j = 0; j <= m; j++) dp[0, j] = j;

        // Dinamik programlama ile mesafe hesaplama
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = char.ToLower(a[i - 1]) == char.ToLower(b[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(
                              Math.Min(dp[i - 1, j] + 1,    // silme maliyeti
                                       dp[i, j - 1] + 1),   // ekleme maliyeti
                              dp[i - 1, j - 1] + cost       // değiştirme maliyeti
                          );
            }
        }
        return dp[n, m];
    }
}
