using Microsoft.AspNetCore.Mvc;

namespace Wellora.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
