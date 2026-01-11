using System;
using System.Collections.Generic;
using TurkSoft.Entities.ServiceEntity.Models;

namespace TurkSoft.Entities.Entities
{
    public class Bank
    {
        public int Id { get; set; }
        public int ExternalBankId { get; set; } // JSON'daki bankId
        public string BankName { get; set; }
        public string Provider { get; set; }
        public string UsernameLabel { get; set; }
        public string PasswordLabel { get; set; }
        public bool RequiresLink { get; set; }
        public bool RequiresTLink { get; set; }
        public bool RequiresAccountNumber { get; set; }
        public string? DefaultLink { get; set; }
        public string? DefaultTLink { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<BankAccount> BankAccounts { get; set; }
        public virtual ICollection<BankCredential> BankCredentials { get; set; }
        public virtual ICollection<BankTransaction> BankTransactions { get; set; }
        public virtual ICollection<TransactionImport> TransactionImports { get; set; }
        public virtual ICollection<MatchingLog> MatchingLogs { get; set; }
    }
}