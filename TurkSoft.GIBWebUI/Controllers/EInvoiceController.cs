using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class EInvoiceController : Controller
    {
        public IActionResult CreateNewInvoice() => View();
        public IActionResult Drafts() => View();
        public IActionResult CreateExportInvoice() => View();
        public IActionResult InboxInvoice() => View();
        public IActionResult OutboxInvoice() => View();
        public IActionResult UploadTransferInvoice() => View();
        public IActionResult GibPortalCancelledOrObjected() => View();
        public IActionResult CreateErpInvoice() => View();
        public IActionResult CreateMarketPlaceInvoice() => View();
    }
}
