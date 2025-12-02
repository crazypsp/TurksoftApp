using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class GibFirmController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
