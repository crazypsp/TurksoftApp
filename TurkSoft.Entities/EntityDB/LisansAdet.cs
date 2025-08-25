using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class LisansAdet:BaseEntity
    {
        public Guid LisansId { get; set; }
        public Lisans Lisans { get; set; }

        public int KuruluCihazSayisi { get; set; }
        public int? Limit { get; set; }
    }
}
