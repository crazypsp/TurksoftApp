using System;

namespace TurkSoft.Entities.Entities
{
    public class BankCredential
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BankId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Encrypt edilecek
        public string Extras { get; set; } // JSON formatında
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedDate { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Bank Bank { get; set; }
    }
}