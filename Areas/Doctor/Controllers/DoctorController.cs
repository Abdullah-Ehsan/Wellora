using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Doctor.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
