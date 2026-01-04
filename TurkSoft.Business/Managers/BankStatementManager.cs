using System.Security.Cryptography;
using System.Text;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Managers
{
    public sealed class BankStatementManager : IBankStatementManager
    {
        private readonly IReadOnlyDictionary<int, IBankStatementProvider> _providers;

        public BankStatementManager(IEnumerable<IBankStatementProvider> providers)
            => _providers = providers.ToDictionary(p => p.BankId, p => p);

        public async Task<List<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            Validate(request);

            if (!_providers.TryGetValue(request.BankId, out var provider))
                throw new InvalidOperationException($"BankId={request.BankId} provider yok.");

            IReadOnlyList<BNKHAR> raw;
            try
            {
                raw = await provider.GetStatementAsync(request, ct);
            }
            catch (Exception ex)
            {
                throw new Exception($"BankaProvider hata. BankId={request.BankId} ({provider.BankCode}). {ex.Message}", ex);
            }

            var rows = raw.ToList();
            Normalize(rows, provider.BankCode, request.AccountNumber);

            // ✅ burada 1’e düşme problemini çözdük
            rows = Deduplicate(rows);

            return rows
                .OrderByDescending(x => x.PROCESSTIME ?? DateTime.MinValue)
                .ThenByDescending(x => x.PROCESSTIMESTR)
                .ToList();
        }

        private static void Validate(BankStatementRequest r)
        {
            if (r.BankId <= 0) throw new ArgumentException("BankId zorunlu");
            if (string.IsNullOrWhiteSpace(r.Username)) throw new ArgumentException("Username zorunlu");
            if (string.IsNullOrWhiteSpace(r.Password)) throw new ArgumentException("Password zorunlu");
            if (string.IsNullOrWhiteSpace(r.AccountNumber)) throw new ArgumentException("AccountNumber zorunlu");
            if (r.EndDate < r.BeginDate) throw new ArgumentException("EndDate < BeginDate olamaz");
        }

        private static void Normalize(List<BNKHAR> rows, string bankCode, string accountNo)
        {
            foreach (var r in rows)
            {
                r.BNKCODE ??= bankCode;
                r.HESAPNO ??= accountNo;

                r.PROCESSAMAOUNT ??= "0";
                r.PROCESSBALANCE ??= "0";
                r.PROCESSDESC ??= "";
                r.PROCESSDESC2 ??= "";
                r.PROCESSDESC3 ??= "";
                r.PROCESSDESC4 ??= "";

                if (r.PROCESSDEBORCRED is not ("A" or "B"))
                {
                    if (decimal.TryParse((r.PROCESSAMAOUNT ?? "0").Replace(",", "."), out var amt))
                        r.PROCESSDEBORCRED = amt >= 0 ? "A" : "B";
                }

                r.Durum ??= 0;
                r.VALUE1 ??= "0";
                r.VALUE2 ??= "0";
                r.CURRUNCY1 ??= "";
                r.CURRUNCY2 ??= "";
                r.TYPECODE ??= "";
                r.REFNO ??= "";
            }
        }

        private static List<BNKHAR> Deduplicate(List<BNKHAR> rows)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<BNKHAR>(rows.Count);

            foreach (var r in rows)
            {
                // ✅ ARTIK PROCESSID tek başına belirleyici değil
                var key = HashRow(r);

                if (seen.Add(key))
                    result.Add(r);
            }

            return result;
        }

        private static string HashRow(BNKHAR r)
        {
            // PROCESSTIME varsa ISO ile ekle => unique artar
            var isoTime = r.PROCESSTIME?.ToString("O") ?? "";

            // Dedupe için daha “zengin” fingerprint:
            var raw = string.Join("|", new[]
            {
                r.BNKCODE ?? "",
                r.HESAPNO ?? "",
                r.PROCESSID ?? "",
                r.PROCESSREFNO ?? "",
                r.PROCESSTIMESTR ?? "",
                r.PROCESSTIMESTR2 ?? "",
                isoTime,
                r.PROCESSAMAOUNT ?? "",
                r.PROCESSBALANCE ?? "",
                (r.PROCESSDESC ?? "").Trim(),
                r.PROCESSTYPECODE ?? "",
                r.TYPECODE ?? "",
                r.REFNO ?? ""
            });

            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));
        }
    }
}
