using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class Lead:BaseEntity,IAuditable
    {
        public string LeadNo { get; set; }
        public Guid BayiId { get; set; }
        public Bayi Bayi { get; set; }

        public string Unvan { get; set; }
        public string Kaynak { get; set; }

        public Guid? SorumluKullaniciId { get; set; }
        public Kullanici SorumluKullanici { get; set; }

        public Adres Adres { get; set; }

        public DateTime OlusturmaTarihi { get; set; }
        public string Notlar { get; set; }

        public Guid CreatedByUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
