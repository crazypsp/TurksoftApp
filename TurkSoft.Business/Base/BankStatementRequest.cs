using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Business.Base
{
    public sealed class BankStatementRequest
    {
        public int BankId { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Link { get; set; }
        public string? TLink { get; set; }
        public Dictionary<string, string> Extras { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string? GetExtra(string key) => Extras.TryGetValue(key, out var v) ? v : null;
        public string GetExtraRequired(string key)
            => GetExtra(key) is { Length: > 0 } v ? v : throw new ArgumentException($"Extras['{key}'] zorunlu.");
    }
}
