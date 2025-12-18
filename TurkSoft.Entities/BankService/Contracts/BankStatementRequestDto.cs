using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.BankService.Contracts
{
    public sealed class BankStatementRequestDto
    {
        public int BankId { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        // bazı bankalar için
        public string? Link { get; set; }
        public string? TLink { get; set; }

        // MusteriNo, FirmaKodu, Profile, TokenEndpoint vs.
        public Dictionary<string, string> Extras { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
