using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class Kullanici:BaseEntity
    {
        public string AdSoyad { get; set; }
        public string Eposta { get; set; }
        public string Sifre { get; set; }
        public string Telefon { get; set; }
        public string Rol { get; set; }
        public string? ProfilResmiUrl { get; set; }
        public ICollection<Log> Loglar { get; set; }
    }
}
