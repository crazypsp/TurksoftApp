using System;

namespace TurkSoft.Entities.Entities
{
    public class BankTransaction
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ValueDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string DebitCredit { get; set; } // D: Borç, C: Alacak
        public decimal BalanceAfterTransaction { get; set; }
        public string ReferenceNumber { get; set; }

        // Eşleştirme Alanları
        public string MatchedClCardCode { get; set; }
        public string MatchedClCardName { get; set; }
        public bool IsMatched { get; set; } = false;
        public DateTime? MatchedDate { get; set; }
        public int? MatchedByUserId { get; set; }

        // Transfer Alanları
        public bool IsTransferred { get; set; } = false;
        public DateTime? TransferredDate { get; set; }
        public int? TransferredByUserId { get; set; }
        public string TransferResult { get; set; }

        // Sistem Alanları
        public int UserId { get; set; }
        public DateTime ImportDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Bank Bank { get; set; }
        public virtual User User { get; set; }
        public virtual User MatchedByUser { get; set; }
        public virtual User TransferredByUser { get; set; }
    }
}