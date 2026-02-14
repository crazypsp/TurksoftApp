using System.ComponentModel.DataAnnotations;

namespace TurkSoft.BankWebUI.ViewModels
{
    public sealed class EditUserVm
    {
        [Required]
        public int Id { get; set; }

        [Required, Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required, Display(Name = "Rol")]
        public string Role { get; set; } = "Viewer";

        [Display(Name = "Aktif Kullanıcı")]
        public bool IsActive { get; set; } = true;
    }
}
