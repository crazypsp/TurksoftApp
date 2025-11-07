using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Bank:BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SwiftCode { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        // Navigation
        [ValidateNever] public ICollection<PaymentAccount> PaymentAccounts { get; set; }
    }
}
