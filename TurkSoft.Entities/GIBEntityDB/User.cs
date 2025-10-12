using System;
using System.Collections.Generic;
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
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; }

        // 🔹 İlişki: 1 kullanıcı birden fazla duyuruyu okumuş olabilir
        public ICollection<UserAnnouncementRead>? UserAnnouncementReads { get; set; }
    }
}
