using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum AktiviteTur { Gorev = 0, Arama = 1, Toplanti = 2, Eposta = 3, Sms = 4 }
    public enum AktiviteDurum { Planlandi = 0, Tamamlandi = 1, Iptal = 2, Gecikti = 3 }
    public class Aktivite:BaseEntity,IAuditable
    {
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public string Konu { get; set; }
        public AktiviteTur Tur { get; set; }
        public AktiviteDurum Durum { get; set; }

        public DateTime? PlanlananTarih { get; set; }
        public DateTime? GerceklesenTarih { get; set; }

        public Guid? IlgiliId { get; set; }    // Lead/Opportunity/Firma/MaliMusavir/Satis...
        public string IlgiliTip { get; set; }

        public Guid? IlgiliKullaniciId { get; set; }
        public Kullanici IlgiliKullanici { get; set; }

        public string Aciklama { get; set; }

        public ICollection<AktiviteAtama> Atamalar { get; set; } = new List<AktiviteAtama>();

        public Guid CreatedByUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
