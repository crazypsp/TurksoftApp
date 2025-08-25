using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class DosyaEk:BaseEntity
    {
        public Guid? IlgiliId { get; set; }
        public string IlgiliTip { get; set; }

        public string DosyaAdi { get; set; }
        public string IcerikTipi { get; set; }
        public string Yol { get; set; }
        public long Boyut { get; set; }
    }
}
