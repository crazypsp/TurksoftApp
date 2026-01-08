using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class ReportsIndexVm
    {
        public ReportFilterVm Filter { get; set; } = new();
        public List<BankTransaction> Rows { get; set; } = new();
        public Dictionary<string, decimal> NetByAccountType { get; set; } = new();
        public Dictionary<string, decimal> NetByDay { get; set; } = new();
    }
}
