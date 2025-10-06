using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ItemsCategory
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public long CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Item Item { get; set; }
        public Category Category { get; set; }
    }
}
