using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Turkcell
{
    public class CommandModel
    {
        public string Command { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SmsCode { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
