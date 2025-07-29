using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class MaliMusavir:BaseEntity
    {
        public string AdSoyad { get; set; }
        public string Eposta { get; set; }
        public string Telefon { get; set; }
        public string Unvan { get; set; }
        public string VergiNo { get; set; }
        public string TCKN { get; set; }
        public ICollection<Firma> Firmalar { get; set; }
        public ICollection<Lisans> Lisanslar { get; set; }
        public ICollection<LisansAdet> LisansAdetleri { get; set; }
        public ICollection<Log> Loglar { get; set; }
    }
}
