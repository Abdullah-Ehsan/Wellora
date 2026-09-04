using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Patient.ViewModels;
using Wellora.Areas.Patient.ViewModels.DoctorInformation;
using Wellora.Data;


namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class DoctorInformationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DoctorInformationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult DoctorListing(string specialty, string language, string gender, int pageNumber = 1)
        {
            int pageSize = 16; // 4 rows × 4 cards

            var doctors = _context.Doctors.Where(d => d.DoctorAvailable == true);

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






        public IActionResult DoctorDetail(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doctor == null)
                return NotFound();

            var patientIdClaim = User.FindFirstValue("CurrentPatientId");

            if (!int.TryParse(patientIdClaim, out int patientId))
                return Unauthorized();

            var patient = _context.Patients
                .FirstOrDefault(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound("Patient profile not found.");

            bool isPrimaryDoctor = patient.PrimaryDoctorId == doctor.DoctorId;


            // Fetch all schedules and breaks for this doctor
            var schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == id && s.IsActive)
                .ToList();

            var breaks = _context.DoctorBreaks
                .Where(b => b.DoctorId == id)
                .ToList();

            // Map day numbers (1–7) to names
            string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            // Build schedule view models for all 7 days
            var scheduleViewModels = new List<DoctorScheduleViewModel>();
            for (int day = 1; day <= 7; day++)
            {
                var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == day);
                var dayBreak = breaks.FirstOrDefault(b => b.DayOfWeek == day);

                if (schedule != null)
                {
                    scheduleViewModels.Add(new DoctorScheduleViewModel
                    {
                        DayOfWeek = dayNames[day - 1],
                        StartTime = DateTime.Today.Add(schedule.StartTime).ToString("hh:mm tt"),
                        EndTime = DateTime.Today.Add(schedule.EndTime).ToString("hh:mm tt"),
                        AppointmentDurationMin = schedule.AppointmentDurationMin,
                        MaxPatientsPerDay = schedule.MaxPatientsPerDay,
                        BreakStart = dayBreak != null ? DateTime.Today.Add(dayBreak.BreakStart).ToString("hh:mm tt") : "—",
                        BreakEnd = dayBreak != null ? DateTime.Today.Add(dayBreak.BreakEnd).ToString("hh:mm tt") : "—"
                    });
                }
                else
                {
                    // No schedule for this day → mark as OFF
                    scheduleViewModels.Add(new DoctorScheduleViewModel
                    {
                        DayOfWeek = dayNames[day - 1],
                        StartTime = "OFF",
                        EndTime = "OFF",
                        AppointmentDurationMin = 0,
                        MaxPatientsPerDay = 0,
                        BreakStart = "—",
                        BreakEnd = "—"
                    });
                }
            }

            // Build main view model
            var viewModel = new DoctorViewModel
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.FullName,
                DateOfBirth = doctor.DateOfBirth,
                Gender = doctor.Gender,
                ProfilePhoto = doctor.ProfilePhoto,
                ContactNumber = doctor.ContactNumber,
                HospitalAddress = doctor.HospitalAddress,
                Country = doctor.Country,
                LicenseNumber = doctor.LicenseNumber,
                PmdcNumber = doctor.PmdcNumber,
                MedicalSchool = doctor.MedicalSchool,
                Certifications = doctor.Certifications,
                Qualifications = doctor.Qualifications,
                PrimaryMedicalDegree = doctor.PrimaryMedicalDegree,
                PostgraduateDegree = doctor.PostgraduateDegree,
                SuperSpecialty = doctor.SuperSpecialty,
                ProfessionalCertification = doctor.ProfessionalCertification,
                AdditionalDegree = doctor.AdditionalDegree,
                YearsExperience = doctor.YearsExperience,
                TelemedicineAvailable = doctor.TelemedicineAvailable,
                ConsultationFee = doctor.ConsultationFee,
                Specialization = doctor.Specialization,
                SubSpecialties = doctor.SubSpecialties,
                ServicesOffered = doctor.ServicesOffered,
                LanguagesSpoken = doctor.LanguagesSpoken,
                Biography = doctor.Biography,
                Achievements = doctor.Achievements,
                Publications = doctor.Publications,
                SocialLinks = doctor.SocialLinks,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt,
                Schedules = scheduleViewModels,
                IsPrimaryDoctor = isPrimaryDoctor
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetPrimaryDoctor(int doctorId)
        {
            var patientIdClaim = User.FindFirstValue("CurrentPatientId");

            if (!int.TryParse(patientIdClaim, out int patientId))
                return Unauthorized();

            var patient = _context.Patients
                .FirstOrDefault(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound("Patient profile not found.");

            var doctorExists = _context.Doctors
                .Any(d => d.DoctorId == doctorId);

            if (!doctorExists)
                return NotFound("Doctor not found.");

            patient.PrimaryDoctorId = doctorId;
            patient.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePrimaryDoctor(int doctorId)
        {
            var patientIdClaim = User.FindFirstValue("CurrentPatientId");

            if (!int.TryParse(patientIdClaim, out int patientId))
                return Unauthorized();

            var patient = _context.Patients
                .FirstOrDefault(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound("Patient profile not found.");

            if (patient.PrimaryDoctorId == doctorId)
            {
                patient.PrimaryDoctorId = null;
                patient.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();
            }

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }

    }

}
