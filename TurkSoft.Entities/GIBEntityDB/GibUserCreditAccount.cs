using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>
    /// Bir GİB firması için kontör hesabı.
    /// </summary>
    public class GibUserCreditAccount : BaseEntity
    {
        public long Id { get; set; }

        public long GibFirmId { get; set; }

        public int TotalCredits { get; set; }
        public int UsedCredits { get; set; }

        // İstersen burada sadece okunur property tanımlayıp EF tarafında [NotMapped] ile configlersin.
        // public int RemainingCredits => TotalCredits - UsedCredits;

        // Navigation
        [ValidateNever] public GibFirm GibFirm { get; set; }
        [ValidateNever] public ICollection<GibUserCreditTransaction> Transactions { get; set; }
    }
}
