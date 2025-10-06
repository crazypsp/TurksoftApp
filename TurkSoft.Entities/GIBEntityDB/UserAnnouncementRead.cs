using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class UserAnnouncementRead
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public DateTime ReadAt { get; set; }
        public int AnnouncementId { get; set; }

        // Navigation
        public User User { get; set; }
        public Announcement Announcement { get; set; }
    }
}
