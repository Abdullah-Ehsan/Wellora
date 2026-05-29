using Microsoft.AspNetCore.Mvc;

namespace Wellora.Areas.Patient.Controllers
{
    public class PatientTransactionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
