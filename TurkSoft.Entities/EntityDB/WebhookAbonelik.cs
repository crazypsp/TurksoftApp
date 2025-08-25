using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class WebhookAbonelik:BaseEntity
    {
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }
        public string Event { get; set; }
        public string Url { get; set; }
        public string IletiGizliAnahtar { get; set; }
    }
}
