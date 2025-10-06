using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Users
    {
        public long Id { get; set; }
        public string Uuid { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateJoined { get; set; }
        public short TypeId { get; set; }
        public bool IsSuperUser { get; set; }
        public DateTime LastLogin { get; set; }
        public string SecureToken { get; set; }
        public string Phone { get; set; }
        public int OtpCode { get; set; }
        public DateTime OtpSendDate { get; set; }
        public bool OtpVerify { get; set; }
        public int CompanyId { get; set; }
        public int DealerId { get; set; }
        public string Image { get; set; }
        public string Email { get; set; }
        public bool MailVerify { get; set; }

        // Navigation
        public ICollection<Request> Requests { get; set; }
        public ICollection<UserAnnouncementRead> UserAnnouncementReads { get; set; }
    }
}
