using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Doctor.ViewModels.PatientAppointment;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class PatientAppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;


        public PatientAppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult PatientAppointmentList(string gender, string search, int page = 1)
        {
            int pageSize = 20;
            var today = DateTime.Today;
            var maxDate = today.AddMonths(7);

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= maxDate);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(a => a.Patient.Gender == gender);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(a => a.Patient.FullName.Contains(search));

            var appointments = query
                .OrderBy(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new PatientAppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    PatientName = a.Patient.FullName,
                    PatientId = a.PatientId,
                    Gender = a.Patient.Gender,
                    Age = DateTime.Today.Year - a.Patient.DateOfBirth.Year -
                          (DateTime.Today.DayOfYear < a.Patient.DateOfBirth.DayOfYear ? 1 : 0),
                    ProfilePhoto = a.Patient.ProfilePhoto,
                    LastVisitedDate = _context.Appointments
                        .Where(x => x.PatientId == a.PatientId && x.AppointmentDate < a.AppointmentDate)
                        .OrderByDescending(x => x.AppointmentDate)
                        .Select(x => (DateTime?)x.AppointmentDate)
                        .FirstOrDefault()
                }).ToList();

            int totalCount = query.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(appointments);
        }
    }
}
