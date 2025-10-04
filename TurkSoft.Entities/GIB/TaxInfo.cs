using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class TaxInfo
    {
        public string TaxTypeCode { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
