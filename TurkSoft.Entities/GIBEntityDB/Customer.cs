using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TaxNo { get; set; }
        public string TaxOffice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<CustomersGroup> CustomersGroups { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
    }
}
