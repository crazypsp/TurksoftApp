using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Class;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;

public class BankaEkstreAnalyzerManager : IBankaEkstreAnalyzerBusiness
{
    // Eşikler
    private const double StrongPickThreshold = 0.55; // güçlü isim adayı
    private const double WeakPickThreshold = 0.35; // kabul edilebilir isim adayı

    // Bonus/cezalar
    private const double PrefixBonus = 0.15; // 120/320 önceliği
    private const double SubstringBonus = 0.15; // adların alt/üstküme ilişkisi
    private const double KeywordCodeBonus = 0.10; // keyword map’in işaret ettiği koda küçük bonus

    // Gürültü/stopword’ler (NormalizeForMatch sonrası büyük harf/ASCII)
    private static readonly HashSet<string> NoiseTokens = new HashSet<string>(StringComparer.Ordinal)
    {
        "FATURA","FATURAYA","FATURASI","FATURANIN","ISTINADEN","ISTINAD","ODEME","ODEMESI","TAHSILAT","TAHSILATI",
        "ISLEM","ISLEMI","HAVALE","EFT","FAST","MAKBUZ","POS","KART","KREDI","BANKA","SUBE","SUBESI","HESAP",
        "NOLU","NO","REF","REFERANS","AÇIKLAMA","ACIKLAMA","A","B","C","D","IBAN","TR","TL","TUTAR","BELGE",
        "FIS","FISI","EDEFTER","VD","VERGI","VDESI","VADESI","VADESINE","MUH","MUHASEBE","HARCAMA","ALIS","SATIS",
        "FTR","FTRSI","DEKONT","SIPARIS","SIPARISI"
    };

    public Task<IDataResult<List<LucaFisRow>>> HareketleriFisSatirlarinaDonusturAsync(
        List<BankaHareket> hareketler,
        List<AccountingCode> hesapKodlari,
        Dictionary<string, string> keywordMap,
        string bankaHesapKodu)
    {
        var fisSatirlari = new List<LucaFisRow>();

        // --- Planı normalize et ---
        var planNorm = (hesapKodlari ?? new List<AccountingCode>())
            .Where(h => !string.IsNullOrWhiteSpace(h?.Code))
            .Select(h => new PlanRow(
                h!.Code!.Trim(),
                h.Name ?? string.Empty,
                NormalizeForMatch(h.Name),
                TokenizeName(NormalizeForMatch(h.Name))
            ))
            .ToList();

        // --- Keyword’leri normalize et ---
        var kwNorm = (keywordMap ?? new Dictionary<string, string>())
            .Where(k => !string.IsNullOrWhiteSpace(k.Key) && !string.IsNullOrWhiteSpace(k.Value))
            .Select(k => new KeyPair(
                NormalizeForMatch(k.Key),
                k.Value.Trim()
            ))
            .ToList();

        // Yardımcı
        bool Is3Digits(string? code) =>
            !string.IsNullOrWhiteSpace(code) && code!.Length == 3 && code.All(char.IsDigit);

        foreach (var h in hareketler ?? Enumerable.Empty<BankaHareket>())
        {
            var aciklama = h?.Aciklama ?? string.Empty;
            var aciklamaNorm = NormalizeForMatch(aciklama);
            var nameTokens = TokenizeName(aciklamaNorm);

            var tutar = h?.Tutar ?? 0m;
            var abs = Math.Abs(tutar);

            string[] preferPrefixes = tutar > 0 ? new[] { "120" } :
                                      tutar < 0 ? new[] { "320" } :
                                      Array.Empty<string>();

            var kw = kwNorm.FirstOrDefault(k => aciklamaNorm.Contains(k.KeyNorm));
            string? keywordTargetCode = kw?.Code;

            // Anahtar kelimeleri içeren bir kod var mı? (öncelikli eşleştirme)
            string? karsiKod = null;
            var matchedKeyword = kwNorm.FirstOrDefault(k => aciklamaNorm.Contains(k.KeyNorm));
            if (matchedKeyword != null)
            {
                // Açıklama içinde keyword varsa, doğrudan eşleştirilen kodu ata
                karsiKod = matchedKeyword.Code;
            }

            // Anahtar kelime eşleşmesi yoksa, isim benzerliği ile devam et
            if (string.IsNullOrEmpty(karsiKod))
                karsiKod = PickBestByName(planNorm, preferPrefixes, aciklamaNorm, nameTokens);
            if (string.IsNullOrEmpty(karsiKod))
                karsiKod = PickBestByName(planNorm, Array.Empty<string>(), aciklamaNorm, nameTokens);

            // Fallback eşleşme adayı (düşük skorları kontrol edeceğiz)
            string? fallbackKod = null;
            bool fallbackIsHighEnough = false;
            if (string.IsNullOrEmpty(karsiKod))
            {
                fallbackKod = FallbackBestName(planNorm, aciklamaNorm, nameTokens);
                if (!string.IsNullOrEmpty(fallbackKod))
                {
                    var bestMatch = planNorm.FirstOrDefault(p => p.Code == fallbackKod);
                    if (bestMatch != null)
                    {
                        double nameSim = Similarity(aciklamaNorm, bestMatch.NameNorm);
                        double tokenSim = TokenOverlap(nameTokens, bestMatch.Tokens);
                        double subsetBonus = (ContainsAsWholePhrase(aciklamaNorm, bestMatch.NameNorm) ||
                                              ContainsAsWholePhrase(bestMatch.NameNorm, aciklamaNorm))
                                              ? SubstringBonus : 0.0;
                        double score = Clamp01(0.65 * nameSim + 0.35 * tokenSim + subsetBonus);
                        fallbackIsHighEnough = score >= WeakPickThreshold;
                    }
                }
            }

            // Eğer hâlâ uygun bir kod yoksa veya fallback skoru düşükse → otomatik atama
            if (string.IsNullOrEmpty(karsiKod))
            {
                if (fallbackIsHighEnough)
                    karsiKod = fallbackKod;
                else
                    karsiKod = tutar > 0 ? "120.01.999" : "320.01.999";
            }


            // Bu işlem için banka kodunu satır içi kopyala
            var bankaKod = bankaHesapKodu;

            // ÖZEL KURAL: bankaHesapKodu ve karsiKod 3 hane ise İKİSİNİ de 771.01 yap
            if (Is3Digits(bankaKod))
            {
                bankaKod = "771.01";
            }
            if (Is3Digits(karsiKod))
            {
                karsiKod = "771.01";
            }
            if (bankaKod == "255.03")
            {
                bankaKod = "771.01";
            }
            if (karsiKod == "255.03")
            {
                karsiKod = "771.01";
            }
            if (tutar == 0m) continue;

            if (tutar > 0m)
            {
                // Para girişi: Banka BORÇ / Karşı ALACAK
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = h!.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = bankaKod,
                    Aciklama = aciklama,
                    Borc = abs,
                    Alacak = 0m
                });
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = h.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = karsiKod,
                    Aciklama = aciklama,
                    Borc = 0m,
                    Alacak = abs
                });
            }
            else // tutar < 0
            {
                // Para çıkışı: Karşı BORÇ / Banka ALACAK
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = h!.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = karsiKod,
                    Aciklama = aciklama,
                    Borc = abs,
                    Alacak = 0m
                });
                fisSatirlari.Add(new LucaFisRow
                {
                    Tarih = h.Tarih,
                    EvrakNo = "TS01",
                    HesapKodu = bankaKod,
                    Aciklama = aciklama,
                    Borc = 0m,
                    Alacak = abs
                });
            }
        }


        return Task.FromResult<IDataResult<List<LucaFisRow>>>(
            new DataResult<List<LucaFisRow>>(fisSatirlari, true, "Fiş satırları başarıyla oluşturuldu."));
    }

    // ----------------------- İsim odaklı seçim -----------------------

    private static string? PickBestByName(
        List<PlanRow> planNorm,
        string[] preferPrefixes,
        string aciklamaNorm,
        HashSet<string> nameTokensInDesc)
    {
        if (planNorm.Count == 0) return null;

        // Önce istenen prefikslerle (varsa) filtrele, yoksa tüm plan
        IEnumerable<PlanRow> pool = planNorm;
        if (preferPrefixes != null && preferPrefixes.Length > 0)
            pool = planNorm.Where(p => StartsWithAny(p.Code, preferPrefixes));

        // Havuz boşsa erken dönüş
        var list = pool.ToList();
        if (list.Count == 0) return null;

        var scored = list.Select(p =>
        {
            double nameSim = Similarity(aciklamaNorm, p.NameNorm);
            double tokenSim = TokenOverlap(nameTokensInDesc, p.Tokens);

            // Alt/üstküme (biri diğerinin içinde açıkça geçiyorsa) bonus
            double subsetBonus = (ContainsAsWholePhrase(aciklamaNorm, p.NameNorm) ||
                                  ContainsAsWholePhrase(p.NameNorm, aciklamaNorm))
                                  ? SubstringBonus : 0.0;

            double total = 0.65 * nameSim + 0.35 * tokenSim + subsetBonus;

            // 120/320 havuzunda zaten olduğumuz için ekstra PrefixBonus eklemiyoruz (çifte sayım olmasın)
            total = Clamp01(total);
            return new Scored(p.Code, p.NameNorm, total);
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        if (scored.Count == 0) return null;

        var best = scored[0];

        // Güçlü eşik -> direkt al
        if (best.Score >= StrongPickThreshold) return best.Code;

        // Zayıf eşik: isim adayı yine de mantıklıysa al
        if (best.Score >= WeakPickThreshold) return best.Code;

        return null;
    }

    private static string? FallbackBestName(
        List<PlanRow> planNorm,
        string aciklamaNorm,
        HashSet<string> nameTokensInDesc)
    {
        if (planNorm.Count == 0) return null;

        var best = planNorm.Select(p =>
        {
            double nameSim = Similarity(aciklamaNorm, p.NameNorm);
            double tokenSim = TokenOverlap(nameTokensInDesc, p.Tokens);
            double subsetBonus = (ContainsAsWholePhrase(aciklamaNorm, p.NameNorm) ||
                                  ContainsAsWholePhrase(p.NameNorm, aciklamaNorm))
                                  ? SubstringBonus : 0.0;
            double total = Clamp01(0.65 * nameSim + 0.35 * tokenSim + subsetBonus);
            return new Scored(p.Code, p.NameNorm, total);
        })
        .OrderByDescending(x => x.Score)
        .FirstOrDefault();

        return best?.Code;
    }

    // ----------------------- Skorlama yardımcıları -----------------------

    private static double Similarity(string aNorm, string bNorm)
    {
        if (string.IsNullOrEmpty(aNorm) && string.IsNullOrEmpty(bNorm)) return 1.0;
        if (string.IsNullOrEmpty(aNorm) || string.IsNullOrEmpty(bNorm)) return 0.0;

        int dist = LevenshteinDistance(aNorm, bNorm);
        int maxLen = Math.Max(aNorm.Length, bNorm.Length);
        return maxLen == 0 ? 1.0 : (1.0 - (double)dist / maxLen);
    }

    // Basit token örtüşmesi (Jaccard)
    private static double TokenOverlap(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        if (a.Count == 0 || b.Count == 0) return 0.0;

        int inter = a.Intersect(b).Count();
        int union = a.Union(b).Count();
        return union == 0 ? 0.0 : (double)inter / union;
    }

    // Açık alt/üstküme kontrolü: birinin stringi diğerinde "kelime sınırlarıyla" geçiyor mu?
    private static bool ContainsAsWholePhrase(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle)) return false;
        // çok kısa/boşluk yok ise direkt Contains
        if (needle.Length <= 3 || !needle.Contains(' '))
            return haystack.Contains(needle, StringComparison.Ordinal);
        // kelime sınırı yaklaşımı
        var h = " " + haystack + " ";
        var n = " " + needle + " ";
        return h.Contains(n, StringComparison.Ordinal);
    }

    private static HashSet<string> TokenizeName(string norm)
    {
        // Sadece harf içeren, sayı/IBAN/stopword olmayan token’ları al
        var tokens = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens
            .Where(t => t.Length >= 2)
            .Where(t => HasLetter(t))
            .Where(t => !IsMostlyDigits(t))
            .Where(t => !LooksLikeIban(t))
            .Where(t => !NoiseTokens.Contains(t))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool HasLetter(string s) => s.Any(ch => char.IsLetter(ch));
    private static bool IsMostlyDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        int digits = s.Count(char.IsDigit);
        return digits >= Math.Max(2, s.Length - 1);
    }
    private static bool LooksLikeIban(string s) =>
        s.Length >= 10 && s.StartsWith("TR", StringComparison.Ordinal) && s.Skip(2).All(char.IsDigit);

    private static bool StartsWithAny(string code, string[] prefixes)
    {
        if (string.IsNullOrEmpty(code) || prefixes == null) return false;
        foreach (var p in prefixes)
            if (!string.IsNullOrEmpty(p) && code.StartsWith(p, StringComparison.Ordinal))
                return true;
        return false;
    }

    private static bool CodesEqualLoose(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        var aa = a.Split(' ')[0];
        var bb = b.Split(' ')[0];
        return aa.Equals(bb, StringComparison.Ordinal);
    }

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

    // --------------------- Normalize & Levenshtein ----------------------

    public static string NormalizeForMatch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var s = input.Trim();
        s = s.Replace('İ', 'I').Replace('ı', 'I')
             .Replace('Ş', 'S').Replace('ş', 'S')
             .Replace('Ç', 'C').Replace('ç', 'C')
             .Replace('Ğ', 'G').Replace('ğ', 'G')
             .Replace('Ü', 'U').Replace('ü', 'U')
             .Replace('Ö', 'O').Replace('ö', 'O');

        s = s.ToUpper(new CultureInfo("tr-TR"));

        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        s = sb.ToString().Normalize(NormalizationForm.FormC);

        var cleaned = new StringBuilder(s.Length);
        foreach (var ch in s)
            cleaned.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        var result = cleaned.ToString();

        while (result.Contains("  ")) result = result.Replace("  ", " ");
        return result.Trim();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        int n = a.Length, m = b.Length;
        var dp = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) dp[i, 0] = i;
        for (int j = 0; j <= m; j++) dp[0, j] = j;

        for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost
                );
            }
        return dp[n, m];
    }

    // ----------------------------- DTO’lar -----------------------------
    private sealed record PlanRow(string Code, string Name, string NameNorm, HashSet<string> Tokens);
    private sealed record KeyPair(string KeyNorm, string Code);
    private sealed record Scored(string Code, string NameNorm, double Score);
}
