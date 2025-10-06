using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class PaymentAccount
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public long BankId { get; set; }
        public string AccountNo { get; set; }
        public string Iban { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Bank Bank { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
