using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class EtiketIliski:BaseEntity
    {
        public Guid EtiketId { get; set; }
        public Etiket Etiket { get; set; }

        public Guid IlgiliId { get; set; }
        public IlgiliTip IlgiliTip { get; set; }
    }
}
