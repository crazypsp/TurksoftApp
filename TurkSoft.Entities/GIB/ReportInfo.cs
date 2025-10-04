using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class ReportInfo
    {
        public string RaporNo { get; set; }
        public string Mukellef { get; set; }
        public string DonemNo { get; set; }
        public string BolumNo { get; set; }
        public DateTime RaporBaslangicTarihi { get; set; }
        public DateTime RaporBitisTarihi { get; set; }
        public string StateCode { get; set; }
        public string StateExplanation { get; set; }
        public int FaturaAdedi { get; set; }
        public int IptalFaturaAdedi { get; set; }
        public int MMAdedi { get; set; }
        public int IptalMMAdedi { get; set; }
        public int SMMAdedi { get; set; }
        public int IptalSMMAdedi { get; set; }
    }
}
