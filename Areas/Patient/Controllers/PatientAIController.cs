using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientAIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
