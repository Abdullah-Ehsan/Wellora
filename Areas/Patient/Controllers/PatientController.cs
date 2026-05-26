using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientController : Controller
    {
        public IActionResult PatientProfile()
        {
            return View();
        }

        public IActionResult PatientDashboard()
        {
            return View();
        }

        public IActionResult AIChat()
        {
            return View();
        }

        public IActionResult Doctorlisting()
        {
            return View();
        }
    }
}
