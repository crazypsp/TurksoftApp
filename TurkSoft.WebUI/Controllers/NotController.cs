using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.WebUI.Controllers
{
  public class NotController : Controller
  {
    public IActionResult Index() => View();
  }
}
