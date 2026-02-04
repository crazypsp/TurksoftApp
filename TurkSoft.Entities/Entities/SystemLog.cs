using System;

namespace TurkSoft.Entities.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string LogLevel { get; set; } = "INFO"; // INFO, WARNING, ERROR
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual User? User { get; set; }
    }
}
