using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public abstract class BaseEntity
    {
        // Kayıt sahibi / kullanıcıya göre mükerrer kontrol için baz alan
        public long UserId { get; set; }              // (veya Guid; User PK’nızla aynı tip)

        // Soft delete
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? DeleteDate { get; set; }
        public long? DeletedByUserId { get; set; }

        // Audit zaman damgaları
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        // (Opsiyonel ama tavsiye)
        public long? CreatedByUserId { get; set; }
        public long? UpdatedByUserId { get; set; }

        // Eşzamanlılık
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

}
