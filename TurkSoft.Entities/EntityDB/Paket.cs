using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Paket:BaseEntity
    {
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        public Guid UrunTipiId { get; set; }
        public UrunTipi UrunTipi { get; set; }
        public ICollection<UrunFiyat> UrunFiyatlar { get; set; } = new List<UrunFiyat>();
        public ICollection<Satis> Satislar { get; set; } = new List<Satis>();
        public ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
        public ICollection<BayiKomisyonTarife> BayiKomisyonTarifeleri { get; set; } = new List<BayiKomisyonTarife>();
        public ICollection<PaketIskonto> PaketIskontolar { get; set; } = new List<PaketIskonto>();
    }
}
