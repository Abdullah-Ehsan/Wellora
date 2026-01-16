using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    public class PatientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AIChat()
        {
            return View();
        }
    }
}
