using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class KeyAccount:BaseEntity
    {
        public string Kod { get; set; }
        public string Aciklama { get; set; }
        public Guid? MaliMusavirId { get; set; }
        public ICollection<MaliMusavir> MaliMusavirs { get; set; } = new List<MaliMusavir>();
    }
}
