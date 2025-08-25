using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class FiyatListesi:BaseEntity
    {
        public string Kod { get; set; }
        public string Ad { get; set; }
        public Guid? BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public DateTime Baslangic { get; set; }
        public DateTime? Bitis { get; set; }

        public ICollection<FiyatListesiKalem> Kalemler { get; set; } = new List<FiyatListesiKalem>();
    }
}
