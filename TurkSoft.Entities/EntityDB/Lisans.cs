using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Lisans:BaseEntity
    {
        public string LisansAnahtari { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public bool YenilendiMi { get; set; }

        public Guid SatisId { get; set; }
        public Satis Satis { get; set; }
    }
}
