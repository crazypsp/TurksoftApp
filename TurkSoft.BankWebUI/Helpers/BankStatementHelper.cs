using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.BankWebUI.Helpers
{
    public static class BankStatementHelper
    {
        // "1.234,56" / "1234.56" vb. güvenli parse
        public static decimal ToDecimalSafe(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0m;

            s = s.Trim();

            // TR formatı olasılığı
            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("tr-TR"), out var tr))
                return tr;

            // invariant
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
                return inv;

            // son çare: virgül/nokta normalize
            var norm = s.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(norm, NumberStyles.Any, CultureInfo.InvariantCulture, out var n2))
                return n2;

            return 0m;
        }

        public static string BuildUniqueKey(int bankId, string accountNo, BNKHAR x)
        {
            var raw = $"{bankId}|{accountNo}|{x.PROCESSTIMESTR}|{x.PROCESSREFNO}|{x.PROCESSAMAOUNT}|{x.PROCESSDEBORCRED}|{x.PROCESSDESC}";
            return Sha256(raw);
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
            return Convert.ToHexString(bytes);
        }
    }
}
