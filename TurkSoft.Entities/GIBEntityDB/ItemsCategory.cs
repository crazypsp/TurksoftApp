using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ItemsCategory: BaseEntity
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public long CategoryId { get; set; }

        // Navigation
        [ValidateNever] public Item Item { get; set; }
        [ValidateNever] public Category Category { get; set; }
    }
}
