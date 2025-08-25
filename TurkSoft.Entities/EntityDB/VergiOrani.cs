using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class VergiOrani:BaseEntity
    {
        public string Kod { get; set; }   // KDV18 vb.
        public decimal Oran { get; set; }
    }
}
