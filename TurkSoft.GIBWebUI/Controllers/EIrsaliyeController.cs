using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class EIrsaliyeController : Controller
    {
        public IActionResult CreateNewDespatch() => View();
        public IActionResult Drafts() => View();
        public IActionResult CreateErpDespatch() => View();
        public IActionResult InboxDespatch() => View();
        public IActionResult OutboxDespatch() => View();
        public IActionResult UploadTransferDespatch() => View();
    }
}
