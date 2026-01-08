using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class AccountingVm
    {
        public string? DateRange { get; set; }
        public string? Bank { get; set; }
        public string? Status { get; set; }

        public List<string> Banks { get; set; } = new();

        public List<TransferRecord> TransferRecords { get; set; } = new();
        public List<AccountMapping> Mappings { get; set; } = new();

        public List<GlAccount> GlAccounts { get; set; } = new();
        public List<Vendor> Vendors { get; set; } = new();

        public List<TransferQueueItem> Queue { get; set; } = new();
        public List<TransferErrorLog> Errors { get; set; } = new();
    }
}
