using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Firma:BaseEntity
    {
        public string FirmaAdi { get; set; }
        public string VergiNo { get; set; }
        public string YetkiliAdSoyad { get; set; }
        public string Telefon { get; set; }
        public string Eposta { get; set; }
        public string Adres { get; set; }

        public Guid? MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }

        public Guid? BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public ICollection<Satis> Satislar { get; set; } = new List<Satis>();
        public ICollection<IletisimKisi> IletisimKisileri { get; set; } = new List<IletisimKisi>();
        public ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
        public ICollection<EntegrasyonHesabi> EntegrasyonHesaplari { get; set; } = new List<EntegrasyonHesabi>();
        public ICollection<Opportunity> Firsatlar { get; set; } = new List<Opportunity>();
        public ICollection<KullaniciFirma> KullaniciBaglantilari { get; set; } = new List<KullaniciFirma>();
    }
}
