using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class InvoiceNumberSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Belge tipi (Fatura, İrsaliye vs.)</summary>
        public long DocumentTypeId { get; set; }

        /// <summary>Şube / Depo (opsiyonel) - Warehouse FK</summary>
        public long? WarehouseId { get; set; }

        /// <summary>Yıl</summary>
        public int Year { get; set; }

        /// <summary>Ön ek (örn: FTR, IRS)</summary>
        public string Prefix { get; set; }

        /// <summary>Son kullanılan numara</summary>
        public long LastNumber { get; set; }

        /// <summary>Kullanıcı bazlı numara mı?</summary>
        public bool IsUserScoped { get; set; }

        /// <summary>Kullanıcı bazlı ise hedef kullanıcı</summary>
        public long? ScopedUserId { get; set; }

        public DocumentType DocumentType { get; set; }
        public Warehouse Warehouse { get; set; }
        public User ScopedUser { get; set; }
    }
}
