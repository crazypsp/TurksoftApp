using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? DeleteDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public long? DeletedByUserId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public long? GibFirmId { get; set; }
        // Navigation
        [ValidateNever] public ICollection<GibFirm> GibFirms { get; set; }
        [ValidateNever] public ICollection<UserRole> UserRoles { get; set; }

        // 🔹 İlişki: 1 kullanıcı birden fazla duyuruyu okumuş olabilir
        [ValidateNever] public ICollection<UserAnnouncementRead>? UserAnnouncementReads { get; set; }
    }
}
