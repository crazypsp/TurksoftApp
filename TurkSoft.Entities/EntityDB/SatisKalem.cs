using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class SatisKalem:BaseEntity
    {
        public Guid SatisId { get; set; }
        public Satis Satis { get; set; }

        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }

        public int Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal Tutar { get; set; }
    }
}
