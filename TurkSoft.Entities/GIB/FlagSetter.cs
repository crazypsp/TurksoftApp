using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class FlagSetter
    {
        public string DocumentDirection { get; set; }
        public string FlagName { get; set; } // ARSIVLENDI, OKUNDU, MUHASEBELESTIRILDI, AKTARILDI, YAZDIRILDI
        public int FlagValue { get; set; } // 0 veya 1
        public string DocumentUUID { get; set; }
    }
}
