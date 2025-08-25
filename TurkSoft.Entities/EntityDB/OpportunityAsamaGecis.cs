using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class OpportunityAsamaGecis:BaseEntity
    {
        public Guid OpportunityId { get; set; }
        public Opportunity Opportunity { get; set; }

        public Guid FromAsamaId { get; set; }
        public OpportunityAsama FromAsama { get; set; }

        public Guid ToAsamaId { get; set; }
        public OpportunityAsama ToAsama { get; set; }

        public DateTime GecisTarihi { get; set; }
    }
}
