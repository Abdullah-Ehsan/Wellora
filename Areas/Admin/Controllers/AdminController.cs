using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
