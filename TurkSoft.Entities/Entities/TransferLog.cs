using System;

namespace TurkSoft.Entities.Entities
{
    public class TransferLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TransactionId { get; set; }
        public string TransferType { get; set; } // SINGLE, BATCH
        public string Status { get; set; } // SUCCESS, FAILED
        public string TargetSystem { get; set; } // LOGO_TIGER
        public string RequestData { get; set; } // JSON formatında
        public string ResponseData { get; set; } // JSON formatında
        public string ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        public string ExternalUniqueKey { get; set; }  // BANK+ACC+DATE+REF+AMOUNT+DC hash/key
        public int? BankId { get; set; }               // internal bank Id (DB)
        public string AccountNumber { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string DebitCredit { get; set; }        // D/C
        public decimal? Amount { get; set; }
        public string BankProcessRefNo { get; set; }   // BNKHAR.PROCESSREFNO
        public int? TigerFicheRef { get; set; }        // LOGICALREF
        // Navigation Properties
        public virtual User User { get; set; }
    }
}