using TurkSoft.BankWebUI.Models;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class BankIntegrationsVm
    {
        public List<BankIntegrationSetting> Items { get; set; } = new();
    }
}
