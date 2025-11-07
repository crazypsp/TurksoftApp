using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Sgk: BaseEntity
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string No { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation
        [ValidateNever] public Invoice Invoice { get; set; }
    }
}
