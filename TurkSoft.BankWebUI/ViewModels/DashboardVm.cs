using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class DashboardVm
    {
        public decimal TotalBalance { get; set; }
        public decimal DailyIn { get; set; }
        public decimal DailyOut { get; set; }
        public int ReconcileAlerts { get; set; }

        public List<BankBalance> BankBalances { get; set; } = new();
        public List<BankTransaction> LatestTransactions { get; set; } = new();
        public List<string> CashflowLabels { get; set; } = new();
        public List<decimal> CashflowNet { get; set; } = new();
    }
}
