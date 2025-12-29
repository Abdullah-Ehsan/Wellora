using Microsoft.AspNetCore.Mvc;

namespace Wellora.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
