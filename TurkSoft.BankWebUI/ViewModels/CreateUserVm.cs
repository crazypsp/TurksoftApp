using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class CreateUserVm
    {
        [Required, Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required, Display(Name = "Rol")]
        public string Role { get; set; } = "Viewer";

        [Required, DataType(DataType.Password), Display(Name = "Geçici Şifre")]
        public string TempPassword { get; set; } = "";

        public List<string> Roles { get; set; } = new() { "Admin", "Finance", "Integrator", "Viewer" };
    }
}
