using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class Firma:BaseEntity
    {
        public string FirmaAdi { get; set; }
        public string VergiNo { get; set; }
        public string YetkiliAdSoyad { get; set; }
        public string Telefon { get; set; }
        public string Eposta { get; set; }
        public string Adres { get; set; }
        public Guid MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }
    }
}
