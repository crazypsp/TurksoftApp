using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ServicesProvider: BaseEntity
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string SystemUser { get; set; }
        public long InvoiceId { get; set; }

        // Navigation
        [ValidateNever] public Invoice Invoice { get; set; }
    }
}
