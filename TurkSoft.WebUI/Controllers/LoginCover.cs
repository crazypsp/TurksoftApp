using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.WebUI.Controllers
{
  public class LoginCover : Controller
  {
    [HttpGet]
    public IActionResult Index()
    {
      return View();
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {      
      return RedirectToAction(nameof(Index)); // LoginCover/Index
    }
  }
}
