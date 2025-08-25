using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Kupon:BaseEntity
    {
        public string Kod { get; set; }
        public Guid? BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public decimal? IndirimYuzde { get; set; }
        public decimal? IndirimTutar { get; set; }
        public int MaksKullanim { get; set; }
        public int Kullanildi { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime? Bitis { get; set; }

        public bool UygunMu(DateTime tarih) => tarih >= Baslangic && (Bitis == null || tarih <= Bitis);
    }
}
