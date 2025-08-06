using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.WebUI.Controllers
{
  public class DashboardsController : Controller
  {
    public IActionResult Index() => View();

    public IActionResult CRM() => View();
  }
}
