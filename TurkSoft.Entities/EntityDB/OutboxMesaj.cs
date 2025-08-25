using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class OutboxMesaj:BaseEntity
    {
        public string Tip { get; set; }        // event type
        public string IcerikJson { get; set; } // NVARCHAR(MAX)
        public bool Islenmis { get; set; }
        public DateTime? IslenmeTarihi { get; set; }
    }
}
