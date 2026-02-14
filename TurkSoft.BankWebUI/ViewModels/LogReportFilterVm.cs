using System;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class LogReportFilterVm
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int? UserId { get; set; }

        public string? Query { get; set; }

        // System logs
        public string? LogLevel { get; set; }

        // Transfer logs
        public string? Status { get; set; }
        public string? TargetSystem { get; set; }
    }
}
