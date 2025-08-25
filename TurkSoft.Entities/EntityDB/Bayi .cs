using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Bayi:BaseEntity
    {
        public string Kod { get; set; }
        public string Unvan { get; set; }
        public string Telefon { get; set; }
        public string Eposta { get; set; }

        public Guid? OlusturanKullaniciId { get; set; }
        public Kullanici OlusturanKullanici { get; set; }

        public BayiFirma BayiFirma { get; set; }

        public ICollection<MaliMusavir> MaliMusavirler { get; set; } = new List<MaliMusavir>();
        public ICollection<Firma> Firmalar { get; set; } = new List<Firma>();
        public ICollection<Satis> Satislar { get; set; } = new List<Satis>();
        public ICollection<BayiKomisyonTarife> KomisyonTarifeleri { get; set; } = new List<BayiKomisyonTarife>();
        public ICollection<SanalPos> SanalPoslar { get; set; } = new List<SanalPos>();
        public ICollection<PaketIskonto> PaketIskontolari { get; set; } = new List<PaketIskonto>();
        public ICollection<FiyatListesi> FiyatListeleri { get; set; } = new List<FiyatListesi>();
        public ICollection<Lead> Leadler { get; set; } = new List<Lead>();
        public ICollection<Opportunity> Firsatlar { get; set; } = new List<Opportunity>();
        public ICollection<Aktivite> Aktiviteler { get; set; } = new List<Aktivite>();
        public ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
        public ICollection<BayiCari> BayiCariler { get; set; } = new List<BayiCari>();
        public ICollection<KomisyonOdemePlani> KomisyonOdemePlanlari { get; set; } = new List<KomisyonOdemePlani>();
        public ICollection<Kupon> Kuponlar { get; set; } = new List<Kupon>();
        public ICollection<WebhookAbonelik> WebhookAbonelikleri { get; set; } = new List<WebhookAbonelik>();
    }
}
