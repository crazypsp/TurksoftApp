using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class UserInvoiceInboxSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Kullanıcı için gelen fatura alias / posta kutusu</summary>
        public string InboxAlias { get; set; }

        /// <summary>VKN/TCKN (gerekirse)</summary>
        public string VknTckn { get; set; }

        /// <summary>Gelen kutusu tipi (EInvoice, EArchive vb.)</summary>
        public string InboxType { get; set; }

        /// <summary>Açıklama</summary>
        public string Description { get; set; }

        /// <summary>Varsayılan gelen kutusu mu?</summary>
        public bool IsDefault { get; set; }
    }
}
