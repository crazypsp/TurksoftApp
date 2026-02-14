using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class ResetPasswordVm
    {
        [Required]
        public int Id { get; set; }

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Şifreyi Onayla")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
