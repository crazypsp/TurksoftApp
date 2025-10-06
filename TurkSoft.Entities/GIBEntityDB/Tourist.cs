using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Tourist
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PassportNo { get; set; }
        public DateTime PassportDate { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string AccountNo { get; set; }
        public string Bank { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public long InvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Invoice Invoice { get; set; }
    }
}
