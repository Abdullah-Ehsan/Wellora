using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Admin.Services.DoctorStats.Interfaces;
using Wellora.Areas.Admin.ViewModels;
using Wellora.Areas.Admin.ViewModels.DoctorInformation;
using Wellora.Areas.Doctor.Models;
using Wellora.Data;
using Wellora.Models;


namespace Wellora.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DoctorInformationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDoctorStatsService _doctorStatsService;
        public DoctorInformationController(ApplicationDbContext context, IDoctorStatsService doctorStatsService)
        {
            _context = context;
            _doctorStatsService = doctorStatsService;
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






        public IActionResult DoctorDetail(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doctor == null)
                return NotFound();

            var user = _context.Users
            .FirstOrDefault(u => u.UserId == doctor.UserId);

            if (user == null)
            {
                return NotFound();
            }





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
                // NEW
                DoctorAvailable = doctor.DoctorAvailable,
                AccountSituation = user.AccountSituation,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt,
                Schedules = scheduleViewModels,
                
            };

            return View(viewModel);
        }



        public async Task<IActionResult> DoctorStats(int doctorId)
        {
            var model = await _doctorStatsService.GetDoctorStatsAsync(doctorId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }


        // =========================================================
        // BAN DOCTOR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanDoctor(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == doctor.UserId);

            if (user == null)
                return NotFound();

            user.AccountSituation = "banned";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }


        // =========================================================
        // UNBAN DOCTOR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanDoctor(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == doctor.UserId);

            if (user == null)
                return NotFound();

            user.AccountSituation = "no_banned";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }


        // =========================================================
        // MAKE DOCTOR AVAILABLE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeDoctorAvailable(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound();

            doctor.DoctorAvailable = true;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }


        // =========================================================
        // MAKE DOCTOR NOT AVAILABLE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeDoctorUnavailable(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound();

            doctor.DoctorAvailable = false;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(DoctorDetail),
                new { id = doctorId }
            );
        }


    }

}
