using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities
{
    public class LisansAdet:BaseEntity
    {
        public int MaxCihazSayisi { get; set; }
        public int KuruluCihazSayisi { get; set; }
        public Guid MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }
    }
}
