using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class ResponseUser
    {
        public string VknTckn { get; set; }
        public string UnvanAd { get; set; }
        public string Etiket { get; set; }
        public string Tip { get; set; }
        public DateTime? IlkKayitZamani { get; set; }
        public DateTime? EtiketKayitZamani { get; set; }
    }
}
