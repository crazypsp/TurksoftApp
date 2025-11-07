using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class StockMovement: BaseEntity
    {
        public long Id { get; set; }
        public long StockId { get; set; }
        public string MovementType { get; set; } // Giriş / Çıkış
        public decimal Quantity { get; set; }
        public DateTime MovementDate { get; set; }

        // Navigation
        [ValidateNever] public Stock Stock { get; set; }
    }
}
