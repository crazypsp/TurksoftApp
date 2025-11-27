using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>
    /// GİB fatura tipi: SATIS, IADE, ISTISNA, TEVKIFAT vb.
    /// </summary>
    public class GibInvoiceType : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Kod: SATIS, IADE vb.</summary>
        public string Code { get; set; }

        /// <summary>Görünen ad</summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActiveForEInvoice { get; set; }

        // Navigation
        [ValidateNever] public ICollection<Invoice> Invoices { get; set; }
    }
}
