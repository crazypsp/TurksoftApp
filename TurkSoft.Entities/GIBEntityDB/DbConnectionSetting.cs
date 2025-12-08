using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class DbConnectionSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Profil adı (örn: Varsayılan, Test, Uretim)</summary>
        public string Name { get; set; }

        /// <summary>Sağlayıcı adı (SqlServer, PostgreSql, MySql...)</summary>
        public string Provider { get; set; }

        /// <summary>Sunucu / Host</summary>
        public string Server { get; set; }

        /// <summary>Veritabanı adı</summary>
        public string Database { get; set; }

        /// <summary>Port (opsiyonel)</summary>
        public int? Port { get; set; }

        /// <summary>SQL login kullanıcı adı (IntegratedSecurity=false ise)</summary>
        public string UserName { get; set; }

        /// <summary>Şifre</summary>
        public string Password { get; set; }

        /// <summary>Windows kimlik doğrulaması kullanılsın mı?</summary>
        public bool IntegratedSecurity { get; set; }

        /// <summary>SSL/TLS kullanılsın mı?</summary>
        public bool UseSsl { get; set; }

        /// <summary>Varsayılan bağlantı mı?</summary>
        public bool IsDefault { get; set; }
    }
}
