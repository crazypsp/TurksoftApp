using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class ReportFilterVm
    {
        [Display(Name = "Tarih Aralığı")]
        public string? DateRange { get; set; }

        [Display(Name = "Hesap Tipi")]
        public string? AccountType { get; set; }

        [Display(Name = "Banka")]
        public string? Bank { get; set; }

        public List<string> Banks { get; set; } = new();
        public List<string> AccountTypes { get; set; } = new();
    }
}
