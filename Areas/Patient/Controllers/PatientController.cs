using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wellora.Areas.Patient.ViewModels;
using Wellora.Data;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        
    }
}
