namespace TurkSoft.BankWebUI.Models
{
    public sealed class DemoUser
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Viewer";
        public bool IsActive { get; set; } = true;
    }

    public sealed class BankIntegrationSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string BankName { get; set; } = "";
        public ConnectionType ConnectionType { get; set; }
        public bool IsActive { get; set; }
        public string PullSchedule { get; set; } = "Her gün 06:00";
        public DateTime? LastPullAt { get; set; }
        public DateTime? NextPullAt { get; set; }
        public BankPullStatus LastStatus { get; set; } = BankPullStatus.Ok;
        public string? LastMessage { get; set; }
        public bool CredentialsConfigured { get; set; }
    }

    public sealed class BankPullLog
    {
        public int Id { get; set; }
        public string SettingId { get; set; } = "";
        public string BankName { get; set; } = "";
        public DateTime FromAt { get; set; }
        public DateTime ToAt { get; set; }
        public BankPullStatus Status { get; set; }
        public string Message { get; set; } = "";
        public int NewTransactionCount { get; set; }
    }

    public sealed class BankBalance
    {
        public string BankName { get; set; } = "";
        public decimal BalanceTry { get; set; }
    }

    public sealed class BankTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string BankName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string ReferenceNo { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Net => Credit - Debit;
    }

    public sealed class GlAccount
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public sealed class Vendor
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public sealed class AccountMapping
    {
        public int Id { get; set; }
        public string BankName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string GlAccountCode { get; set; } = "";
        public string GlAccountName { get; set; } = "";
        public string? CostCenter { get; set; }
        public bool IsDefault { get; set; }
    }

    public sealed class TransferRecord
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string BankName { get; set; } = "";
        public DateTime Date { get; set; }
        public string AccountType { get; set; } = "";
        public string ReferenceNo { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public string? GlAccountCode { get; set; }
        public string? GlAccountName { get; set; }
        public string? CostCenter { get; set; }

        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }

        public TransferRecordStatus Status { get; set; } = TransferRecordStatus.Draft;

        public bool IsMapped => !string.IsNullOrWhiteSpace(GlAccountCode);
    }

    public sealed class TransferQueueItem
    {
        public int Id { get; set; }
        public int TransferRecordId { get; set; }
        public QueueStatus Status { get; set; }
        public int AttemptCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? LastError { get; set; }
    }

    public sealed class TransferErrorLog
    {
        public int Id { get; set; }
        public int QueueItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BankName { get; set; } = "";
        public string ReferenceNo { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
