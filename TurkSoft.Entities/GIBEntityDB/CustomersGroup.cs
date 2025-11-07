using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CustomersGroup:BaseEntity
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public long GroupId { get; set; }

        // Navigation
        [ValidateNever] public Customer Customer { get; set; }
        [ValidateNever] public Group Group { get; set; }
    }
}
