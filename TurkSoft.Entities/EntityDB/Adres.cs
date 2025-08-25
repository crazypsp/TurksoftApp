using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    // Owned tip – bazı varlıklarda kullanılır
    public class Adres:BaseEntity
    {
        public string Ulke { get; set; }
        public string Sehir { get; set; }
        public string Ilce { get; set; }
        public string PostaKodu { get; set; }
        public string AcikAdres { get; set; }
    }
}
