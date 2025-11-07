using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class PurchaseItem: BaseEntity
    {
        public long Id { get; set; }
        public long PurchaseId { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        // Navigation
        [ValidateNever] public Purchase Purchase { get; set; }
        [ValidateNever] public Item Item { get; set; }
    }
}
