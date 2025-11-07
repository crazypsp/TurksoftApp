using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Payment: BaseEntity
    {
        public long Id { get; set; }
        public long PaymentTypeId { get; set; }
        public long PaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }

        // Navigation
        [ValidateNever] public PaymentType PaymentType { get; set; }
        [ValidateNever] public PaymentAccount PaymentAccount { get; set; }
        [ValidateNever] public ICollection<InvoicesPayment> InvoicesPayments { get; set; }
    }
}
