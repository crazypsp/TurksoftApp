using System;

namespace TurkSoft.Entities.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string LogLevel { get; set; } // INFO, WARNING, ERROR
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }
        public string IpAddress { get; set; }
        public string ActionName { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}