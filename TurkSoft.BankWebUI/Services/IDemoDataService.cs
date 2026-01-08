using TurkSoft.BankWebUI.Models;
using TurkSoft.BankWebUI.ViewModels;

namespace TurkSoft.BankWebUI.Services
{
    public interface IDemoDataService
    {
        DemoUser? ValidateUser(string email, string password);

        DashboardVm GetDashboard();
        ReportsIndexVm GetReports(ReportFilterVm filter);

        UsersIndexVm GetUsers();
        void AddUser(CreateUserVm vm);

        BankIntegrationsVm GetBankIntegrations();
        void ToggleBankActive(string id);
        void PullNow(string id);

        BankPullLogsVm GetBankPullLogs(string? bank);
        BankPullDeltaVm GetDeltaByLogId(int logId);

        AccountingVm GetAccounting(string? dateRange, string? bank, string? status);
        void ApplyMapping(int transferId, string glCode, string? costCenter, string? vendorCode);
        void EnqueueTransfer(int transferId);
        void ExportQueueItem(int queueId);
        void RetryQueueItem(int queueId);
        void ClearError(int errorId);
    }
}
