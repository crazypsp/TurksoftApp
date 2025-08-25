using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class PaketIskonto:BaseEntity
    {
        public Guid PaketId { get; set; }
        public Paket Paket { get; set; }
        public Guid? BayiId { get; set; }
        public Bayi Bayi { get; set; }
        public decimal IskontoYuzde { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime? Bitis { get; set; }
    }
}
