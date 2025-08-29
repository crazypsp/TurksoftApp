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
  }
}
