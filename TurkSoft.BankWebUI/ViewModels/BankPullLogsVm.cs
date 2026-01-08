using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class BankPullLogsVm
    {
        public string? Bank { get; set; }
        public List<string> Banks { get; set; } = new();
        public List<BankPullLog> Logs { get; set; } = new();
    }
}
