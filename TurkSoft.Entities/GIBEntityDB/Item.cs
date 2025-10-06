using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public long BrandId { get; set; }
        public long UnitId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Brand Brand { get; set; }
        public Unit Unit { get; set; }
        public ICollection<ItemsCategory> ItemsCategories { get; set; }
        public ICollection<ItemsDiscount> ItemsDiscounts { get; set; }
        public ICollection<Identifiers> Identifiers { get; set; }
    }
}
