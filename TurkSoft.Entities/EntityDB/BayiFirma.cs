using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class BayiFirma:BaseEntity
    {
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public string VergiNo { get; set; }
        public string Iban { get; set; }
        public string Adres { get; set; }
    }
}
