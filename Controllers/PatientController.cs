using Microsoft.AspNetCore.Mvc;

namespace Wellora.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
