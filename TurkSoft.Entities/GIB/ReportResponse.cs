using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class ReportResponse
    {
        public List<ReportInfo> Reports { get; set; } = new();
        public int Code { get; set; }
        public string Explanation { get; set; }
        public string Cause { get; set; }
    }
}
