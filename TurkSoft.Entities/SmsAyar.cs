using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class SmsAyar:BaseEntity
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string GondericiAdi { get; set; }
    }
}
