using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    /// <summary>Kullanıcı ↔ Firma pivotu</summary>
    public class KullaniciFirma:BaseEntity
    {
        public Guid KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        public Guid FirmaId { get; set; }
        public Firma Firma { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;
        public string? AtananRol { get; set; }           // "FirmaAdmin","Muhasebe","IK" vb.
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
    }
}
