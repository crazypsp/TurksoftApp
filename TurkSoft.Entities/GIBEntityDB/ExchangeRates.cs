using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ExchangeRates: BaseEntity
    {
        public short Id { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Buying { get; set; }
        public decimal Selling { get; set; }
        public string Name { get; set; }
    }
}
