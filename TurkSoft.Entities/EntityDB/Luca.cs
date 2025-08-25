using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Luca:BaseEntity
    {
        public string UyeNo { get; set; }
        public string KullaniciAdi { get; set; }
        public string Parola { get; set; }
        public ICollection<MaliMusavir> MaliMusavirs { get; set; }
    }
}
