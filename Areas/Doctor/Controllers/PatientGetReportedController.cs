using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class PatientGetReportedController : Controller
    {
        public IActionResult PatientReported()
        {
            return View();
        }
    }
}
