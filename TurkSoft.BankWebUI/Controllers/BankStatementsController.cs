using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.BankWebUI.Controllers
{
    public class BankStatementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
