using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class ReportsIndexVm
    {
        public ReportFilterVm Filter { get; set; }
        public List<BankTransactionVm> Rows { get; set; }
        public Dictionary<string, decimal> NetByAccountType { get; set; }
        public Dictionary<string, decimal> NetByDay { get; set; }
    }
}
