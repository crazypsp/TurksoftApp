using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class InvoicesTax: BaseEntity
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        // Navigation
        [ValidateNever] public Invoice Invoice { get; set; }
    }
}
