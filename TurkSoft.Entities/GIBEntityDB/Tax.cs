using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Tax: BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal Rate { get; set; }

        // Navigation
        [ValidateNever] public ICollection<InvoicesTax> InvoicesTaxes { get; set; }
    }
}
