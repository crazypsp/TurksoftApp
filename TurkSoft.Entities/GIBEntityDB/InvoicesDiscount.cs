using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class InvoicesDiscount: BaseEntity
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Base { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        // Navigation
        [ValidateNever] public Item Item { get; set; }
    }
}
