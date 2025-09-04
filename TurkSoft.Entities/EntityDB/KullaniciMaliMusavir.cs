using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    /// <summary>Kullanıcı ↔ MaliMüşavir pivotu</summary>
    public class KullaniciMaliMusavir : BaseEntity
    {
        public Guid KullaniciId { get; set; }
        public Kullanici? Kullanici { get; set; } = null!;

        public Guid MaliMusavirId { get; set; }
        public MaliMusavir? MaliMusavir { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;
        public string? AtananRol { get; set; }           // "MMAdmin","Stajyer","Personel" vb.
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
    }
}
