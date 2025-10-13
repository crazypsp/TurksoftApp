using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class EDovizController : Controller
    {
        public IActionResult CreateNewDoviz() => View();
        public IActionResult Drafts() => View();
        public IActionResult OutboxDoviz() => View();
        public IActionResult ReportSendingRequest() => View();
        public IActionResult SendingReports() => View();
    }
}
