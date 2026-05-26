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

        public IActionResult DoctorListing(string specialty, string language, string gender, int pageNumber = 1)
        {
            int pageSize = 16; // 4 rows × 4 cards

            var doctors = _context.Doctors.AsQueryable();

            // Apply filters with normalization
            if (!string.IsNullOrEmpty(specialty))
            {
                var normalizedSpecialty = specialty.Replace(" ", "_");
                doctors = doctors.Where(d => d.Specialization == normalizedSpecialty);
            }

            if (!string.IsNullOrEmpty(language))
            {
                var normalizedLanguage = language.Replace(" ", "_");
                doctors = doctors.Where(d => d.LanguagesSpoken != null && d.LanguagesSpoken.Contains(normalizedLanguage));
            }


            if (!string.IsNullOrEmpty(gender))
                doctors = doctors.Where(d => d.Gender == gender);

            // Pagination
            var totalDoctors = doctors.Count();
            var totalPages = (int)System.Math.Ceiling(totalDoctors / (double)pageSize);

            var pagedDoctors = doctors
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new DoctorListingViewModel
            {
                Doctors = pagedDoctors,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                SelectedSpecialty = specialty,
                SelectedLanguage = language,
                SelectedGender = gender
            };

            // If AJAX request, return partial only
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DoctorCardsPartial", viewModel);

            return View(viewModel);

        }
    }
}
