using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class BankPullDeltaVm
    {
        public int LogId { get; set; }
        public string BankName { get; set; } = "";
        public DateTime FromAt { get; set; }
        public DateTime ToAt { get; set; }
        public List<BankTransaction> Transactions { get; set; } = new();
    }
}
