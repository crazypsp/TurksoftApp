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
        public Customer Customer { get; set; }
        public ICollection<InvoicesItem> InvoicesItems { get; set; }
        public ICollection<InvoicesTax> InvoicesTaxes { get; set; }
        public ICollection<InvoicesDiscount> InvoicesDiscounts { get; set; }
        public ICollection<Tourist> Tourists { get; set; }
        public ICollection<Sgk> SgkRecords { get; set; }
        public ICollection<ServicesProvider> ServicesProviders { get; set; }
        public ICollection<Returns> Returns { get; set; }
        public ICollection<InvoicesPayment> InvoicesPayments { get; set; }
    }
}
