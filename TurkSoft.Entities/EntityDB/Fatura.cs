using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum FaturaTip { Satis = 0, Iade = 1 }
    public enum BelgeDurum { Taslak = 0, Kesildi = 1, Iptal = 2 }
    public class Fatura:BaseEntity,IAuditable
    {
        public string FaturaNo { get; set; }

        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public Guid? SatisId { get; set; }
        public Satis Satis { get; set; }

        public Guid FirmaId { get; set; }
        public Firma Firma { get; set; }

        public FaturaTip Tip { get; set; }
        public BelgeDurum Durum { get; set; }
        public DateTime FaturaTarihi { get; set; }

        public decimal Kdvoran { get; set; }
        public decimal Kdvtutar { get; set; }
        public decimal Toplam { get; set; }
        public decimal Net { get; set; }

        public ICollection<FaturaKalem> Kalemler { get; set; } = new List<FaturaKalem>();

        public Guid CreatedByUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
