using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
                var key = !string.IsNullOrWhiteSpace(r.PROCESSID)
                    ? $"PID:{r.PROCESSID}"
                    : $"H:{HashRow(r)}";

                if (seen.Add(key))
                    result.Add(r);
            }

            return result;
        }

        private static string HashRow(BNKHAR r)
        {
            var raw = $"{r.BNKCODE}|{r.HESAPNO}|{r.PROCESSTIMESTR}|{r.PROCESSAMAOUNT}|{r.PROCESSDESC}|{r.PROCESSREFNO}";
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));
        }
    }
}
