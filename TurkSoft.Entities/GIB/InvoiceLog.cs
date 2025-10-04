using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class InvoiceLog
    {
        public string DocumentUUID { get; set; }
        public string EnvelopeUUID { get; set; }
        public DateTime ProcessTime { get; set; }
        public string ProcessState { get; set; }
        public string ProcessResult { get; set; }
        public string ResultExplanation { get; set; }
    }
}
