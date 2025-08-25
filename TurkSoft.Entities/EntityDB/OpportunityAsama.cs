using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class OpportunityAsama:BaseEntity
    {
        public string Kod { get; set; }    // NEW, QUAL, PROP, WON, LOST...
        public string Ad { get; set; }
        public decimal OlasilikYuzde { get; set; }
    }
}
