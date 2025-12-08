using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class EmailSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Profil adı (örn: Genel Mail, Fatura Maili)</summary>
        public string Name { get; set; }

        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }

        /// <summary>Kimlik doğrulama için kullanıcı adı</summary>
        public string UserName { get; set; }

        /// <summary>Şifre</summary>
        public string Password { get; set; }

        /// <summary>Gönderici e-posta adresi</summary>
        public string FromAddress { get; set; }

        /// <summary>Gönderici görünen ad</summary>
        public string FromDisplayName { get; set; }

        public bool UseDefaultCredentials { get; set; }

        /// <summary>Varsayılan e-posta profili mi?</summary>
        public bool IsDefault { get; set; }
    }
}
