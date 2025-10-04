using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class LogResponse
    {
        public int QueryState { get; set; }
        public string StateExplanation { get; set; }
        public int LogCount { get; set; }
        public List<InvoiceLog> InvoiceLogs { get; set; } = new();
    }
}
