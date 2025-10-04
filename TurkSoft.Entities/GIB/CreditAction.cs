using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class CreditAction
    {
        public DateTime PurchaseDate { get; set; }
        public int PurchaseCount { get; set; }
        public int ConsideredCount { get; set; }
        public int CreditUnit { get; set; }
        public string CreditPoolId { get; set; }
        public string BuyerVknTckn { get; set; }
        public string ActionType { get; set; } // Enum olarak da tanımlanabilir
    }
}
