using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult CompanyInformations() => View();
        public IActionResult UserSettings() => View();
        public IActionResult Settings() => View();
        public IActionResult DocumentStates() => View();
    }
}
