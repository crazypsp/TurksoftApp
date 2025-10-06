using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Payment
    {
        public long Id { get; set; }
        public long PaymentTypeId { get; set; }
        public long PaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public PaymentType PaymentType { get; set; }
        public PaymentAccount PaymentAccount { get; set; }
        public ICollection<InvoicesPayment> InvoicesPayments { get; set; }
    }
}
