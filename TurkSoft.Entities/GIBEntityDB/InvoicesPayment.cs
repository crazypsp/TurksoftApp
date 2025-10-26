using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class InvoicesPayment
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public long PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ValidateNever] public Invoice Invoice { get; set; }
        [ValidateNever] public Payment Payment { get; set; }
    }
}
