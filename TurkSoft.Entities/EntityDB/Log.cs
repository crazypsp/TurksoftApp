using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Log:BaseEntity
    {
        public string Islem {  get; set; }
        public string IpAdres { get; set; }
        public string Tarayici { get; set; }
        public Guid? KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }
        public Guid? MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }
    }
}
