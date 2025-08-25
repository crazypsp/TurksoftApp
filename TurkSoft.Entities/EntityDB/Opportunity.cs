using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum OpportunityDurum { Acik = 0, Kazanildi = 1, Kaybedildi = 2 }
    public class Opportunity:BaseEntity,IAuditable
    {
        public string FirsatNo { get; set; }

        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public Guid? MaliMusavirId { get; set; }
        public MaliMusavir MaliMusavir { get; set; }

        public Guid? FirmaId { get; set; }
        public Firma Firma { get; set; }

        public Guid AsamaId { get; set; }
        public OpportunityAsama Asama { get; set; }

        public decimal TahminiTutar { get; set; }
        public OpportunityDurum Durum { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
        public ICollection<OpportunityAsamaGecis> AsamaGecisleri { get; set; } = new List<OpportunityAsamaGecis>();

        public Guid CreatedByUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
