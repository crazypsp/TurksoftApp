using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class EArchiveController : Controller
    {
        public IActionResult CreateNewEarchiveInvoice() => View();
        public IActionResult Drafts() => View();
        public IActionResult InboxEarchiveInvoice() => View();
        public IActionResult OutboxEarchiveInvoice() => View();
        public IActionResult UploadTransferEarchiveInvoice() => View();
        public IActionResult ReportSendingRequest() => View();
        public IActionResult SendingReports() => View();
        public IActionResult EArchiveEmailStatusTrack() => View();
        public IActionResult CreateErpEArchiveInvoice() => View();
    }
}
