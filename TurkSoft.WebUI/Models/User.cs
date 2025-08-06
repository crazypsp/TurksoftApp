using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Models
{
  public class User
  {
    [Display(Name = "Adı Soyadı")]
    [Required]
    public string NameLastname { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Şifre")]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; }

    [Display(Name = "Ülke")]
    public int CountryId { get; set; }
    
    

    // View’da dropdown için kullanacağız
    public IEnumerable<SelectListItem> Countries { get; set; }
        = new List<SelectListItem>();
    [Display(Name = "Cinsiyet")]
    [Required]
    public IEnumerable<SelectListItem> Gender { get; set; }
     = new List<SelectListItem>();
  }
}
