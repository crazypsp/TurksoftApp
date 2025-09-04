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
        public string Telefon { get; set; }
        public string Eposta { get; set; }
        public string Unvan { get; set; }
        public string VergiNo { get; set; }
        public string TCKN { get; set; }

        public Guid? BayiId { get; set; }
        public Bayi? Bayi { get; set; }

        public ICollection<Firma>? Firmalar { get; set; } = new List<Firma>();
        public ICollection<Satis>? Satislar { get; set; } = new List<Satis>();
        public ICollection<EntegrasyonHesabi>? EntegrasyonHesaplari { get; set; } = new List<EntegrasyonHesabi>();
        public ICollection<Opportunity>? Firsatlar { get; set; } = new List<Opportunity>();
        public ICollection<KullaniciMaliMusavir>? KullaniciBaglantilari { get; set; } = new List<KullaniciMaliMusavir>();
    }
}
