using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
