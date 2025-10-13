using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ListsController : Controller
    {
        public IActionResult MukellefList() => View();
        public IActionResult MalzemeHizmetList() => View();
        public IActionResult MusteriCariList() => View();
        public IActionResult TaxList() => View();
        public IActionResult ExceptionalCodes() => View();
        public IActionResult TevkifatCodes() => View();
        public IActionResult ExportRegisteredCodes() => View();
        public IActionResult SpecialBaseCodes() => View();
        public IActionResult DurumKod() => View();
        public IActionResult FiyatList() => View();
        public IActionResult TransferList() => View();
    }
}
