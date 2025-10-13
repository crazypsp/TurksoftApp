using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ESmmController : Controller
    {
        public IActionResult CreateNewReceipt() => View();
        public IActionResult Drafts() => View();
        public IActionResult CreateErpReceipt() => View();
        public IActionResult OutboxReceipt() => View();
        public IActionResult UploadTransferSmm() => View();
        public IActionResult ReportSendingRequest() => View();
        public IActionResult SendingReports() => View();
    }
}
