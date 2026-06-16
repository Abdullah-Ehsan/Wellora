using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area ("Doctor")]
    [Authorize (Roles = "doctor")]
    public class PatientGetDiagnoseController : Controller
    {
        public IActionResult PatientDiagnoses()
        {
            return View();
        }
    }
}
