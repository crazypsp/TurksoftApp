using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ECheckController : Controller
    {
        public IActionResult CreateNewCheck() => View();
        public IActionResult Drafts() => View();
        public IActionResult OutboxCheck() => View();
        public IActionResult ReportSendingRequest() => View();
        public IActionResult SendingReports() => View();
    }
}
