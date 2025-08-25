using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum SatisDurumu { Teklif = 0, Kesildi = 1, Iptal = 2 }
    public class Satis:BaseEntity
    {
        public string SatisNo { get; set; }
        public DateTime SatisTarihi { get; set; }

        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public Guid MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }

        public Guid? FirmaId { get; set; }
        public Firma Firma { get; set; }

        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }

        public decimal KDVOrani { get; set; }
        public decimal KDVTutar { get; set; }
        public decimal IskontoTutar { get; set; }
        public decimal ToplamTutar { get; set; }
        public decimal NetTutar { get; set; }

        public SatisDurumu SatisDurumu { get; set; }

        public ICollection<SatisKalem> Kalemler { get; set; } = new List<SatisKalem>();
        public ICollection<Lisans> Lisanslar { get; set; } = new List<Lisans>();
        public ICollection<Odeme> Odemeler { get; set; } = new List<Odeme>();
        public ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
    }
}
