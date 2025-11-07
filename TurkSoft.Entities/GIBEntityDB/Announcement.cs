using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Announcement: BaseEntity
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Status { get; set; }

        // Navigation
        [ValidateNever] public User User { get; set; }
        [ValidateNever] public ICollection<UserAnnouncementRead> UserAnnouncementReads { get; set; }
    }
}
