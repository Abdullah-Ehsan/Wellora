using Microsoft.AspNetCore.Mvc;

namespace Wellora.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
