using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class SanalPos:BaseEntity
    {
        public Guid? BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public string Saglayici { get; set; }   // PayTR, Garanti, IsBank...
        public string BaseApiUrl { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string PosAnahtar { get; set; }
        public decimal? StandartKomisyonYuzde { get; set; }

        public ICollection<Odeme> Odemeler { get; set; } = new List<Odeme>();
    }
}
