using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class UserAnnouncementRead: BaseEntity
    {
        public int Id { get; set; }
        public DateTime ReadAt { get; set; }
        public int AnnouncementId { get; set; }

        // Navigation
        [ValidateNever] public User User { get; set; } = default!;
        [ValidateNever] public Announcement Announcement { get; set; }
    }
}
