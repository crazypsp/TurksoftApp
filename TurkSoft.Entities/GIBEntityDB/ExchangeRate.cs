using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ExchangeRate: BaseEntity
    {
        public long Id { get; set; }
        public long CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }

        // Navigation
        [ValidateNever] public Currency Currency { get; set; }
    }
}
