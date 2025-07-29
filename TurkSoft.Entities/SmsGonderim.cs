using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class SmsGonderim:BaseEntity
    {
        public string AliciNumara { get; set; }
        public string Mesaj { get; set; }
        public bool BasariliMi { get; set; }
    }
}
