using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class AktiviteAtama:BaseEntity
    {
        public Guid AktiviteId { get; set; }
        public Aktivite Aktivite { get; set; }

        public Guid KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }

        public string Rol { get; set; } // Owner/Particip
    }
}
