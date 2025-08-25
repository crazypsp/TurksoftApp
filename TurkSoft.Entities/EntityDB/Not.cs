using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum NotTip { Genel = 0, Satis = 1, Firsat = 2, Firma = 3 }
    public class Not:BaseEntity
    {
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public NotTip Tip { get; set; }
        public Guid? IlgiliId { get; set; }
        public string IlgiliTip { get; set; }

        public Guid OlusturanUserId { get; set; }
        public Kullanici Olusturan { get; set; }
    }
}
