using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    // === ÖDEME & SANAL POS ===
    public enum OdemeYontemi { HavaleEft = 0, KrediKartiSanalPos = 1, PayTR = 2, SanalKart = 3 }
    public enum OdemeDurumu { Beklemede = 0, Basarili = 1, Basarisiz = 2, Iade = 3 }
    public class Odeme:BaseEntity
    {
        public Guid SatisId { get; set; }
        public Satis Satis { get; set; }

        public DateTime OdemeTarihi { get; set; }
        public OdemeYontemi OdemeYontemi { get; set; }
        public OdemeDurumu OdemeDurumu { get; set; }

        public decimal Tutar { get; set; }
        public decimal? KomisyonOrani { get; set; }
        public decimal? KomisyonTutar { get; set; }
        public decimal NetTutar { get; set; }

        public Guid? SanalPosId { get; set; }
        public SanalPos SanalPos { get; set; }

        public string SaglayiciIslemNo { get; set; }
        public string Taksit { get; set; }
        public string Aciklama { get; set; }
    }
}
