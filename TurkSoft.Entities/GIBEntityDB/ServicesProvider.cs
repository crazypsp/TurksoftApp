using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ServicesProvider
    {
        public int Id { get; set; }
        public string No { get; set; }
        public string SystemUser { get; set; }
        public long InvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Invoice Invoice { get; set; }
    }
}
