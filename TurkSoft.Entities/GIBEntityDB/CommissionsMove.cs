using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CommissionsMove: BaseEntity
    {
        public long Id { get; set; }
        public int DealerId { get; set; }
        public decimal Price { get; set; }
        public string OrderIds { get; set; } // JSON

        // Navigation
        [ValidateNever] public Dealer Dealer { get; set; }
        [ValidateNever] public User User { get; set; }
    }
}
