using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Luca
{
    public class LucaLogMessage
    {
        public string Message { get; set; }
        public string Status { get; set; } //Info,Success, Error
        public DateTime Timestamp { get; set; }=DateTime.Now;
    }
}
