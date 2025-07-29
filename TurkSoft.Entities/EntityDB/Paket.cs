using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Paket:BaseEntity
    {
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        public Guid UrunTipiId { get; set; }
        public UrunTipi UrunTipi { get; set; }
    }
}
