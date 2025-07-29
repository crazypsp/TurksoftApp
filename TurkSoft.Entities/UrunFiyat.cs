using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class UrunFiyat:BaseEntity
    {
        public decimal Fiyat { get; set; }
        public string ParaBirimi { get; set; }
        public DateTime GecerlilikTarihi { get; set; }
        public Guid UrunTipiId { get; set; }
        public UrunTipi UrunTipi { get; set; }
    }
}
