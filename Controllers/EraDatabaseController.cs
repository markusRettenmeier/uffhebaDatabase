using Microsoft.AspNetCore.Mvc;

namespace Sammlerplattform.Controllers
{
    public class EraDatabaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
