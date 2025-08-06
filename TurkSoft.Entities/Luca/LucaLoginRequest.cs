using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Luca
{
    public class LucaLoginRequest
    {
        public string CustumerNo { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
    }
}
