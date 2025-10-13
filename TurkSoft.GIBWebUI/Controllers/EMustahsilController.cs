using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class EMustahsilController : Controller
    {
        public IActionResult CreateNewCreditNote() => View();
        public IActionResult Drafts() => View();
        public IActionResult CreateErpCreditNote() => View();
        public IActionResult OutboxCreditNote() => View();
        public IActionResult UploadTransferMustahsil() => View();
        public IActionResult ReportSendingRequest() => View();
        public IActionResult SendingReports() => View();
    }
}
