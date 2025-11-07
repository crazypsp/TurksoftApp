using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Purchase: BaseEntity
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public string PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }

        // Navigation
        [ValidateNever] public Supplier Supplier { get; set; }
        [ValidateNever] public ICollection<PurchaseItem> PurchaseItems { get; set; }
    }
}
