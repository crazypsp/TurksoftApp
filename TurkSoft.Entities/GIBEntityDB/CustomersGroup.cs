using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CustomersGroup
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public long GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Customer Customer { get; set; }
        public Group Group { get; set; }
    }
}
