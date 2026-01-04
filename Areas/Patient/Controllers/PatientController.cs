using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
