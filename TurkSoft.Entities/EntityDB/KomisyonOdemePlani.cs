using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum KomisyonOdemeDurumu { Planlandi = 0, Odendi = 1, Kapandi = 2 }
    public class KomisyonOdemePlani:BaseEntity
    {
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public int DonemYil { get; set; }
        public int DonemAy { get; set; }
        public decimal ToplamKomisyon { get; set; }
        public KomisyonOdemeDurumu Durum { get; set; }
    }
}
