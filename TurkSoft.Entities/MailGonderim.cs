using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class MailGonderim:BaseEntity
    {
        public string Alici { get; set; }
        public string Konu { get; set; }
        public string Icerik { get; set; }
        public bool BasariliMi { get; set; }
    }
}
