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

        // Navigation Properties
        public virtual User User { get; set; }
    }
}