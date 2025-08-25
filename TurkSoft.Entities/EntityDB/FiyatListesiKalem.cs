using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class FiyatListesiKalem:BaseEntity
    {
        public Guid FiyatListesiId { get; set; }
        public FiyatListesi FiyatListesi { get; set; }

        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }

        public decimal BirimFiyat { get; set; }
    }
}
