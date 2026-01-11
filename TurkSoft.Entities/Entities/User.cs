using System;
using System.Collections.Generic;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Entities.ServiceEntity.Models;

namespace TurkSoft.Entities.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<BankCredential> BankCredentials { get; set; }
        public virtual ICollection<BankTransaction> BankTransactions { get; set; }
        public virtual ICollection<TransactionImport> TransactionImports { get; set; }
        public virtual ICollection<MatchingLog> MatchingLogs { get; set; }
        public virtual ICollection<TransferLog> TransferLogs { get; set; }
        public virtual ICollection<ExportLog> ExportLogs { get; set; }
        public virtual ICollection<SystemLog> SystemLogs { get; set; }
    }
}