using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    // === ENTEGRASYON HESAPLARI ===
    public enum EntegrasyonSistemTipi { Luca = 0, Logo = 1, Mikro = 2, Netsis = 3, ETA = 4, Zirve = 5, Diger = 99 }
    public class EntegrasyonHesabi:BaseEntity
    {
        public EntegrasyonSistemTipi SistemTipi { get; set; }

        // DB bağlantısı
        public string Host { get; set; }
        public string VeritabaniAdi { get; set; }
        public string KullaniciAdi { get; set; }
        public string Parola { get; set; }

        // API bağlantısı
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Guid? MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }

        public Guid? FirmaId { get; set; }
        public Firma Firma { get; set; }

        public string Aciklama { get; set; }
    }
}
