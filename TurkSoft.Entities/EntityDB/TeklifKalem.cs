using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class TeklifKalem:BaseEntity
    {
        public Guid TeklifId { get; set; }
        public Teklif Teklif { get; set; }

        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }

        public int Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal Tutar { get; set; }
    }
}
