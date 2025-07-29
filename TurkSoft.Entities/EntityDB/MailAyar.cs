using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public class MailAyar:BaseEntity
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Eposta { get; set; }
        public string Sifre { get; set; }
        public bool SSLKullan { get; set; }
    }
}
