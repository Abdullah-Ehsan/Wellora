using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wellora.Areas.Admin.ViewModels.PatientInformation;
using Wellora.Data;
using Wellora.Models;

namespace Wellora.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class PatientInformationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientInformationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult PatientListing(string gender, int pageNumber = 1)
        {
            int pageSize = 16; // 4 rows × 4 cards

            var patients = _context.Patients.AsQueryable();

            // Filter by gender
            if (!string.IsNullOrEmpty(gender))
            {
                patients = patients.Where(p => p.Gender == gender);
            }

            // Pagination
            var totalPatients = patients.Count();
            var totalPages = (int)Math.Ceiling(totalPatients / (double)pageSize);

            var pagedPatients = patients
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new PatientListingViewModel
            {
                Patients = pagedPatients,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                SelectedGender = gender
            };

            // If AJAX request, return partial only
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PatientCardsPartial", viewModel);
            }

            return View(viewModel);
        }

        public IActionResult PatientDetail(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == patient.UserId);

            var viewModel = new PatientViewModel
            {
                // Basic Information
                PatientId = patient.PatientId,
                UserId = patient.UserId,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Address = patient.Address,
                ProfilePhoto = patient.ProfilePhoto,

                // Emergency Information
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,

                // Medical Information
                BloodGroup = patient.BloodGroup,
                Allergies = patient.Allergies,
                MedicalConditions = patient.MedicalConditions,
                Medications = patient.Medications,

                // Doctor Information
                PrimaryDoctorId = patient.PrimaryDoctorId,

                // Preferences
                PreferredLanguage = patient.PreferredLanguage,

                // User Account Information
                Email = user?.Email,
                Username = user?.FirstName,
                AccountSituation = user?.AccountSituation,

                // Dates
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };

            return View(viewModel);
        }

        // =========================================================
        // BAN PATIENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanPatient(int patientId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == patient.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.AccountSituation = "banned";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(PatientDetail),
                new { id = patientId }
            );
        }

        // =========================================================
        // UNBAN PATIENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanPatient(int patientId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == patient.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.AccountSituation = "no_banned";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(PatientDetail),
                new { id = patientId }
            );
        }
    }
}