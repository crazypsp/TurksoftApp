using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class ELedgerController : Controller
    {
        public IActionResult ELedgerUpload() => View();
        public IActionResult CreateErpELedger() => View();
        public IActionResult UploadTransferELedger() => View();
        public IActionResult ELedgerList() => View();
        public IActionResult ELedgerTransfer() => View();
        public IActionResult ELedgerSettings() => View();
    }
}
