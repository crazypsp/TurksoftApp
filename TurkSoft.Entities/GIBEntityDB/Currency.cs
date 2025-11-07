using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Currency: BaseEntity
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }

        // Navigation
        [ValidateNever] public ICollection<ExchangeRate> ExchangeRates { get; set; }
    }
}
