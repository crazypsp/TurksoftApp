using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class PaymentAccount:BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public long BankId { get; set; }
        public string AccountNo { get; set; }
        public string Iban { get; set; }
        public string Currency { get; set; }

        // Navigation
        [ValidateNever] public Bank Bank { get; set; }
        [ValidateNever] public ICollection<Payment> Payments { get; set; }
    }
}
