using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Invoice : BaseEntity
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public int Type { get; set; }

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

        // 🔹 Yeni: Log ve kredi hareketleri navigasyonları
        [ValidateNever] public ICollection<GibInvoiceOperationLog> GibInvoiceOperationLogs { get; set; }
        [ValidateNever] public ICollection<GibUserCreditTransaction> GibUserCreditTransactions { get; set; }
    }
}
