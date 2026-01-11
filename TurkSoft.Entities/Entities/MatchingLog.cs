using System;

namespace TurkSoft.Entities.Entities
{
    public class MatchingLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BankId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public int MatchedCount { get; set; }
        public int UnmatchedCount { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string MatchingCriteria { get; set; } // JSON formatında

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Bank Bank { get; set; }
    }
}