using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class AdminPanelController : Controller
    {
        public IActionResult UserList() => View();
        public IActionResult ManagementPanel() => View();
        public IActionResult Requests() => View();
        public IActionResult CreditReport() => View();
        public IActionResult KullaniciBazliHizmetReport() => View();
        public IActionResult Notifications() => View();
        public IActionResult GetTraining() => View();
    }
}
