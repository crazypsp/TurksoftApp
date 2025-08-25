using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum IlgiliTip { Lead = 0, Opportunity = 1, Satis = 2, Firma = 3, MaliMusavir = 4 }
    public class Etiket:BaseEntity
    {
        public string Ad { get; set; }
        public ICollection<EtiketIliski> Iliskiler { get; set; } = new List<EtiketIliski>();
    }
}
