using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class XsltListResponse
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public bool IsDefault { get; set; }
    }
}
