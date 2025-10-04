using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class ReportRequest
    {
        public string Mukellef { get; set; }
        public string DonemNo { get; set; }
        public string BolumNo { get; set; }
        public DateTime BolumBaslangicTarihi { get; set; }
        public DateTime BolumBitisTarihi { get; set; }
        public byte[] XmlContent { get; set; }
    }
}
