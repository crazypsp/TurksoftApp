using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class UrunTipi:BaseEntity
    {
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        public ICollection<Paket> Paketler { get; set; }
        public ICollection<UrunFiyat> UrunFiyatlari { get; set; }
    }
}
