using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    public class PatientProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
