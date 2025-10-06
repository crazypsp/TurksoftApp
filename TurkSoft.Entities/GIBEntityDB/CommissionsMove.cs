using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CommissionsMove
    {
        public long Id { get; set; }
        public int DealerId { get; set; }
        public decimal Price { get; set; }
        public long UserId { get; set; }
        public string OrderIds { get; set; } // JSON
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Dealer Dealer { get; set; }
        public User User { get; set; }
    }
}
