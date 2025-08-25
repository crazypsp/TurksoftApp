using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class BayiCari:BaseEntity
    {
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public decimal Bakiye { get; set; }
        public ICollection<BayiCariHareket> Hareketler { get; set; } = new List<BayiCariHareket>();
    }
}
