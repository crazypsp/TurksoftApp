using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class IletisimKisi:BaseEntity
    {
        public Guid FirmaId { get; set; }
        public Firma Firma { get; set; }

        public string AdSoyad { get; set; }
        public string Eposta { get; set; }
        public string Telefon { get; set; }

        public Adres Adres { get; set; }
    }
}
