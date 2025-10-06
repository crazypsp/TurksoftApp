using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Identifiers
    {
        public short Id { get; set; }
        public string Uuid { get; set; }
        public string Desc { get; set; }
        public string Value { get; set; }
        public short Type { get; set; }
        public long ItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Item Item { get; set; }
    }
}
