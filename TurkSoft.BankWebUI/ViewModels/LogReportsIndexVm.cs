using System.Collections.Generic;
using TurkSoft.Entities.Entities;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class LogReportsIndexVm
    {
        public LogReportFilterVm Filter { get; set; } = new LogReportFilterVm();

        public List<TransferLog> TransferLogs { get; set; } = new List<TransferLog>();
        public List<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

        public bool IsAdmin { get; set; }
        public int CurrentUserId { get; set; }
    }
}
