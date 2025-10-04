using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class UserQueryResponse
    {
        public int QueryState { get; set; }
        public string StateExplanation { get; set; }
        public int UserCount { get; set; }
        public List<ResponseUser> Users { get; set; } = new();
    }
}
