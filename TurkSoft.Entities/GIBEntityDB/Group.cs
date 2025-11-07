using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Group: BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        // Navigation
        [ValidateNever] public ICollection<CustomersGroup> CustomersGroups { get; set; }
    }
}
