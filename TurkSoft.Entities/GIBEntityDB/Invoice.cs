using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Invoice
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ValidateNever] public Customer Customer { get; set; }
        [ValidateNever] public ICollection<InvoicesItem> InvoicesItems { get; set; }
        [ValidateNever] public ICollection<InvoicesTax> InvoicesTaxes { get; set; }
        [ValidateNever] public ICollection<InvoicesDiscount> InvoicesDiscounts { get; set; }
        [ValidateNever] public ICollection<Tourist> Tourists { get; set; }
        [ValidateNever] public ICollection<Sgk> SgkRecords { get; set; }
        [ValidateNever] public ICollection<ServicesProvider> ServicesProviders { get; set; }
        [ValidateNever] public ICollection<Returns> Returns { get; set; }
        [ValidateNever] public ICollection<InvoicesPayment> InvoicesPayments { get; set; }
    }
}
