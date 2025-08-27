using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    /// <summary>Kullanıcı ↔ Bayi pivotu (ek alanlarla)</summary>
    public class KullaniciBayi:BaseEntity
    {
        public Guid KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;     // Kullanıcının birincil bayisi?
        public string? AtananRol { get; set; }           // "BayiAdmin","Satis","Destek" vb.
        public DateTime? BaslangicTarihi { get; set; }   // Yetki başlangıcı
        public DateTime? BitisTarihi { get; set; }       // Yetki bitişi
    }
}
