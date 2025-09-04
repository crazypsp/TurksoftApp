using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    /// <summary>
    /// Sistem kullanıcısı. Artık Bayi/Firma/MaliMüşavir bağlantıları navigation ile tutuluyor.
    /// </summary>
    public class Kullanici:BaseEntity
    {
        public string AdSoyad { get; set; }
        public string Eposta { get; set; }
        public string Sifre { get; set; }
        public string Telefon { get; set; }
        public string Rol { get; set; }
        public string? ProfilResmiUrl { get; set; }
        public Guid? OlusturanKullaniciId { get; set; }
        // === İlişkiler (çok-çok) ===
        public ICollection<KullaniciBayi> BayiBaglantilari { get; set; } = new List<KullaniciBayi>();
        public ICollection<KullaniciFirma> FirmaBaglantilari { get; set; } = new List<KullaniciFirma>();
        public ICollection<KullaniciMaliMusavir> MaliMusavirBaglantilari { get; set; } = new List<KullaniciMaliMusavir>();
    }
}
