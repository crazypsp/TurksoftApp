using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class WhatsappAyar:BaseEntity
    {
        public string ApiUrl { get; set; }
        public string Token { get; set; }
        public string Numara { get; set; }
    }
}
