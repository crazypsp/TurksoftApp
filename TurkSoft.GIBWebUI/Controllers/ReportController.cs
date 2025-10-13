using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult ReportCustomize() => View();
        public IActionResult GeneralReport() => View();
        public IActionResult DeservedReport() => View();
    }
}
