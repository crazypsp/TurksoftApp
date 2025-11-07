using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Item: BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public long BrandId { get; set; }
        public long UnitId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }

        // Navigation
        [ValidateNever] public Brand Brand { get; set; }
        [ValidateNever] public Unit Unit { get; set; }
        [ValidateNever] public ICollection<ItemsCategory> ItemsCategories { get; set; }
        [ValidateNever] public ICollection<ItemsDiscount> ItemsDiscounts { get; set; }
        [ValidateNever] public ICollection<Identifiers> Identifiers { get; set; }
    }
}
