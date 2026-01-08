using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class LoginVm
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
        public string? Error { get; set; }
    }
}
