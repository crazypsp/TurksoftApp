using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>
    /// GİB senaryosu: TEMELFATURA, TICARIFATURA, EARSIVFATURA vb.
    /// </summary>
    public class GibInvoiceScenario : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Kod: TEMELFATURA, TICARIFATURA vb.</summary>
        public string Code { get; set; }

        /// <summary>Görünen ad</summary>
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation
        [ValidateNever] public ICollection<Invoice> Invoices { get; set; }
    }
}
