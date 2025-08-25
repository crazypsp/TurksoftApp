using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum CariHareketTip { Borc = 0, Alacak = 1 }
    public class BayiCariHareket:BaseEntity
    {
        public Guid BayiCariId { get; set; }
        public BayiCari BayiCari { get; set; }

        public DateTime IslemTarihi { get; set; }
        public CariHareketTip Tip { get; set; }
        public decimal Tutar { get; set; }
        public string? Aciklama { get; set; }
        public Guid? ReferansId { get; set; }
        public string? ReferansTip { get; set; }
    }
}
