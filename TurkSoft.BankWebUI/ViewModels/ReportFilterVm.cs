using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class ReportFilterVm
    {
        public string DateRange { get; set; }
        public string AccountType { get; set; }
        public string Bank { get; set; }
        public string Account { get; set; }

        // Select list'ler için
        public List<SelectListItem> Banks { get; set; }
        public List<SelectListItem> Accounts { get; set; }
        public List<string> AccountTypes { get; set; }
    }
}
