using System;

namespace TurkSoft.Entities.Entities
{
    public class TransactionImport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BankId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public string Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string ErrorMessage { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string RequestParameters { get; set; } // JSON formatında

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Bank Bank { get; set; }
    }
}