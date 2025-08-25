using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum TeklifDurumu { Taslak = 0, Gonderildi = 1, Kabul = 2, Reddedildi = 3, SureDoldu = 4 }
    public class Teklif:BaseEntity,IAuditable
    {
        public string TeklifNo { get; set; }
        public Guid OpportunityId { get; set; }
        public Opportunity Opportunity { get; set; }

        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }

        public decimal Kdvoran { get; set; }
        public decimal Kdvtutar { get; set; }
        public decimal Toplam { get; set; }
        public decimal Net { get; set; }
        public TeklifDurumu Durum { get; set; }
        public DateTime? GecerlilikBitis { get; set; }

        public ICollection<TeklifKalem> Kalemler { get; set; } = new List<TeklifKalem>();

        public Guid CreatedByUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
