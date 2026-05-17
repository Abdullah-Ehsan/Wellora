using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
