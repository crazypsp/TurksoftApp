using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class PrefixCodeResponse
    {
        public string PrefixType { get; set; }
        public string PrefixKey { get; set; }
        public bool Active { get; set; }
        public bool IsOK { get; set; }
    }
}
