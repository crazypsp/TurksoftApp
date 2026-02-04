using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Entities.BankService.Models
{
    public partial class BNKHAR
    {
        public string ExternalUniqueKey { get; set; }
        public bool IsTransferred { get; set; }
        public int? TigerFicheRef { get; set; }
    }
}
