using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Doctor.Services.DoctorProfile.Interfaces;
using Wellora.Areas.Doctor.Services.DoctorProfile.Services;
using Wellora.Areas.Doctor.ViewModels.DoctorProfile;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class DoctorProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IWebHostEnvironment _environment;


        public DoctorProfileController(ApplicationDbContext context,
            IDoctorScheduleService scheduleService,
            IWebHostEnvironment environment)
        {   
            _context = context;
            _doctorScheduleService = scheduleService;
            _environment = environment;
        }

        // =====================================================
        // MAIN DOCTOR PROFILE PAGE
        // =====================================================

        [HttpGet]
        public IActionResult DoctorProfile()
        {
            return View(
                "~/Areas/Doctor/Views/DoctorProfile/DoctorProfile.cshtml"
            );
        }


        // =====================================================
        // GET CURRENT DOCTOR ID FROM CLAIM
        // =====================================================

        private int GetDoctorId()
        {
            var claim = User.FindFirst("CurrentDoctorId")?.Value;

            if (string.IsNullOrWhiteSpace(claim))
            {
                throw new UnauthorizedAccessException(
                    "CurrentDoctorId claim is missing."
                );
            }

            if (!int.TryParse(claim, out var doctorId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid CurrentDoctorId claim."
                );
            }

            return doctorId;
        }

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );
        }


        // =====================================================
        // SCHEDULE PARTIAL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> SchedulePartial(
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            var model = await _doctorScheduleService.GetScheduleAsync(
                doctorId,
                cancellationToken);

            return PartialView("_Schedule", model);
        }



        // =====================================================
        // UPDATE SCHEDULE (STANDARD FORM POST)
        // =====================================================

        /***
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(
            DoctorScheduleUpdateViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();
            var userId = GetUserId();

            // Never trust DoctorId coming from the browser.
            model.DoctorId = doctorId;

            

            // =====================================================
            // CONVERT ScheduleStatus -> ScheduleRows.Status
            // =====================================================

            for (int i = 0; i < model.ScheduleRows.Count; i++)
            {
                var status =
                    i < model.ScheduleStatus.Count
                        ? model.ScheduleStatus[i]
                        : null;

                model.ScheduleRows[i].Status =
                    status switch
                    {
                        "On" => true,
                        "Off" => false,
                        _ => null
                    };
            }

            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x =>
                        x.Value!.Errors.Select(e =>
                            $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                TempData["ErrorMessage"] =
                    errors.Count > 0
                        ? string.Join(" | ", errors)
                        : "Please correct the invalid schedule information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            // =====================================================
            // UPDATE SCHEDULE
            // =====================================================

            var result =
                await _doctorScheduleService.UpdateScheduleAsync(
                    doctorId,
                    userId,
                    model,
                    cancellationToken);

            // =====================================================
            // HANDLE FAILURE
            // =====================================================

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;

                return RedirectToAction(nameof(DoctorProfile));
            }

            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(DoctorProfile));


        }

        ***/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(
            DoctorScheduleUpdateViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            // ---------------------------------------------------------
            // Convert ScheduleStatus into ScheduleRows[i].Status
            // ---------------------------------------------------------

            for (int i = 0; i < model.ScheduleRows.Count; i++)
            {
                var status =
                    i < model.ScheduleStatus.Count
                        ? model.ScheduleStatus[i]
                        : null;

                model.ScheduleRows[i].Status =
                    status == "On";
            }

            // ---------------------------------------------------------
            // Basic validation
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the invalid schedule information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            // ---------------------------------------------------------
            // Get existing schedule
            // ---------------------------------------------------------

            var existingSchedules =
                await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync(cancellationToken);

            var existingBreaks =
                await _context.DoctorBreaks
                    .Where(b => b.DoctorId == doctorId)
                    .ToListAsync(cancellationToken);

            // ---------------------------------------------------------
            // Delete current schedule and breaks
            // ---------------------------------------------------------

            _context.DoctorSchedules.RemoveRange(existingSchedules);
            _context.DoctorBreaks.RemoveRange(existingBreaks);

            // ---------------------------------------------------------
            // Add ON schedules
            // ---------------------------------------------------------

            foreach (var row in model.ScheduleRows
                .Where(r => r.Status == true))
            {
                if (!row.StartTime.HasValue ||
                    !row.EndTime.HasValue)
                {
                    continue;
                }

                _context.DoctorSchedules.Add(
                    new DoctorSchedule
                    {
                        DoctorId = doctorId,

                        DayOfWeek = row.DayOfWeek,

                        StartTime = row.StartTime.Value,

                        EndTime = row.EndTime.Value,

                        AppointmentDurationMin =
                            row.AppointmentDurationMin ?? 30,

                        MaxPatientsPerDay =
                            row.MaxPatientsPerDay ?? 1,

                        BufferTimeMin =
                            row.BufferTimeMin ?? 0
                    });
            }

            // ---------------------------------------------------------
            // Add breaks
            // ---------------------------------------------------------

            foreach (var breakRow in model.Breaks)
            {
                if (!breakRow.BreakStart.HasValue ||
                    !breakRow.BreakEnd.HasValue)
                {
                    continue;
                }

                // Only save breaks for ON days
                var scheduleIsOn =
                    model.ScheduleRows.Any(
                        r =>
                            r.DayOfWeek == breakRow.DayOfWeek &&
                            r.Status == true);

                if (!scheduleIsOn)
                {
                    continue;
                }

                _context.DoctorBreaks.Add(
                    new DoctorBreak
                    {
                        DoctorId = doctorId,

                        DayOfWeek = breakRow.DayOfWeek,

                        BreakStart = breakRow.BreakStart.Value,

                        BreakEnd = breakRow.BreakEnd.Value
                    });
            }

            // ---------------------------------------------------------
            // Save
            // ---------------------------------------------------------

            await _context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] =
                "Schedule updated successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }





        // =====================================================
        // Clinical PARTIAL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ClinicalInfoPartial(
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctor.DoctorId,

                HospitalAddress = doctor.HospitalAddress,
                LicenseNumber = doctor.LicenseNumber,
                PmdcNumber = doctor.PmdcNumber,
                Country = doctor.Country,
                MedicalSchool = doctor.MedicalSchool,
                Certifications = doctor.Certifications,

                TelemedicineAvailable = doctor.TelemedicineAvailable,
                ConsultationFee = doctor.ConsultationFee
            };

            return PartialView("_ClinicalInfo", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClinicalInfo(
            DoctorDetailsViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            // Never trust DoctorId from the browser.
            model.DoctorId = doctorId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the invalid clinical information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            // Clinical / regulatory information
            doctor.HospitalAddress = model.HospitalAddress;
            doctor.LicenseNumber = model.LicenseNumber;
            doctor.PmdcNumber = model.PmdcNumber;
            doctor.Country = model.Country;
            doctor.MedicalSchool = model.MedicalSchool;
            doctor.Certifications = model.Certifications;

            // Consultation settings
            doctor.TelemedicineAvailable = model.TelemedicineAvailable;
            doctor.ConsultationFee = model.ConsultationFee;

            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] =
                "Clinical information updated successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }



        // =====================================================
        // VIEW ACHIEVEMENTS & BIOGRAPHY
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> AchievementsPartial(
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctor.DoctorId,

                Achievements = doctor.Achievements,
                Publications = doctor.Publications,
                Biography = doctor.Biography
            };

            return PartialView("_Achievements", model);
        }

        // =====================================================
        // UPDATE ACHIEVEMENTS & BIOGRAPHY
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAchievements(
            DoctorDetailsViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            // Never trust DoctorId coming from the browser.
            model.DoctorId = doctorId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the invalid achievements and biography information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            // Update achievements information
            doctor.Achievements = model.Achievements;
            doctor.Publications = model.Publications;
            doctor.Biography = model.Biography;

            // Update timestamp
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] =
                "Achievements and biography updated successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }




        // =====================================================
        // VIEW LANGUAGES & CONTACT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ContactPartial(
    CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctor.DoctorId,
                LanguagesSpoken = doctor.LanguagesSpoken,
                ContactNumber = doctor.ContactNumber,
                SocialLinks = doctor.SocialLinks
            };

            // =====================================================
            // SPLIT SAVED CONTACT NUMBER
            // =====================================================

            if (!string.IsNullOrWhiteSpace(doctor.ContactNumber))
            {
                var number = doctor.ContactNumber.Trim();

                if (number.StartsWith("+92"))
                {
                    model.CountryCode = "+92";
                    model.PhoneNumber = number.Substring(3);
                }
                else if (number.StartsWith("+1"))
                {
                    model.CountryCode = "+1";
                    model.PhoneNumber = number.Substring(2);
                }
                else if (number.StartsWith("+44"))
                {
                    model.CountryCode = "+44";
                    model.PhoneNumber = number.Substring(3);
                }
                else if (number.StartsWith("+49"))
                {
                    model.CountryCode = "+49";
                    model.PhoneNumber = number.Substring(3);
                }
                else
                {
                    model.PhoneNumber = number;
                }
            }

            return PartialView("_Contact", model);
        }



        // =====================================================
        // UPDATE LANGUAGES & CONTACT
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(
            DoctorDetailsViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            model.DoctorId = doctorId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the invalid contact information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            // =====================================================
            // BUILD CONTACT NUMBER
            // =====================================================

            var countryCode = model.CountryCode?.Trim() ?? "";
            var phoneNumber = model.PhoneNumber?.Trim() ?? "";

            countryCode = countryCode.Replace(" ", "");
            phoneNumber = phoneNumber
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                // Remove leading + from country code if necessary
                countryCode = countryCode.TrimStart('+');

                // Remove leading 0 from phone number if you want
                // the stored format to be +923001234567
                phoneNumber = phoneNumber.TrimStart('0');

                model.ContactNumber =
                    "+" + countryCode + phoneNumber;
            }
            else
            {
                model.ContactNumber = null;
            }

            // =====================================================
            // UPDATE CONTACT INFORMATION
            // =====================================================

            doctor.LanguagesSpoken = model.LanguagesSpoken;
            doctor.ContactNumber = model.ContactNumber;
            doctor.SocialLinks = model.SocialLinks;

            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] =
                "Contact information updated successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }






        // =====================================================
        // VIEW SPECIALIZATION & SERVICES
        // =====================================================

        // =====================================================
        // VIEW SPECIALIZATION & EDUCATION
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> SpecializationPartial(
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctor.DoctorId,

                // =====================================================
                // SPECIALIZATION
                // =====================================================

                Specialization = doctor.Specialization,
                SubSpecialties = doctor.SubSpecialties,
                ServicesOffered = doctor.ServicesOffered,
                YearsExperience = doctor.YearsExperience,


                // =====================================================
                // EDUCATION
                // =====================================================

                PrimaryMedicalDegree = doctor.PrimaryMedicalDegree,
                PostgraduateDegree = doctor.PostgraduateDegree,
                SuperSpecialty = doctor.SuperSpecialty,
                ProfessionalCertification = doctor.ProfessionalCertification,
                AdditionalDegree = doctor.AdditionalDegree
            };

            return PartialView("_Specialization", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSpecialization(
            DoctorDetailsViewModel model,
            CancellationToken cancellationToken)
        {
            var doctorId = GetDoctorId();

            // Never trust DoctorId coming from the browser.
            model.DoctorId = doctorId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the invalid specialization information.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken
                );

            if (doctor == null)
            {
                TempData["ErrorMessage"] =
                    "Doctor profile could not be found.";

                return RedirectToAction(nameof(DoctorProfile));
            }

            // =====================================================
            // SPECIALIZATION
            // =====================================================

            doctor.Specialization = model.Specialization;
            doctor.SubSpecialties = model.SubSpecialties;
            doctor.ServicesOffered = model.ServicesOffered;
            doctor.YearsExperience = model.YearsExperience;


            // =====================================================
            // EDUCATION
            // =====================================================

            doctor.PrimaryMedicalDegree = model.PrimaryMedicalDegree;
            doctor.PostgraduateDegree = model.PostgraduateDegree;
            doctor.SuperSpecialty = model.SuperSpecialty;
            doctor.ProfessionalCertification = model.ProfessionalCertification;
            doctor.AdditionalDegree = model.AdditionalDegree;


            // =====================================================
            // UPDATED TIMESTAMP
            // =====================================================

            doctor.UpdatedAt = DateTime.UtcNow;


            // =====================================================
            // SAVE
            // =====================================================

            await _context.SaveChangesAsync(cancellationToken);


            TempData["SuccessMessage"] =
                "Specialization and education information updated successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }


        //loas the main user profile contain the basic info
        [HttpGet]
        public async Task<IActionResult> ProfilePartial()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctorIdClaim = User.FindFirstValue("CurrentDoctorId");

            if (!int.TryParse(userIdClaim, out int userId) ||
                !int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == doctorId &&
                    d.UserId == userId);

            if (user == null || doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorProfileViewModel
            {
                UserId = user.UserId,
                DoctorId = doctor.DoctorId,

                // User information
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,

                // Doctor information
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                ProfilePhotoPath = doctor.ProfilePhoto
            };

            return PartialView("_Profile", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(DoctorProfileViewModel model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctorIdClaim = User.FindFirstValue("CurrentDoctorId");

            if (!int.TryParse(userIdClaim, out int userId) ||
                !int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == doctorId &&
                    d.UserId == userId);

            if (user == null || doctor == null)
            {
                return NotFound();
            }


            // =====================================================
            // UPDATE USER INFORMATION
            // =====================================================

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Username = model.Username;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.Now;


            // =====================================================
            // UPDATE DOCTOR INFORMATION
            // =====================================================

            doctor.Gender = model.Gender;
            doctor.DateOfBirth = model.DateOfBirth;
            doctor.UpdatedAt = DateTime.Now;


            // =====================================================
            // UPDATE PROFILE PHOTO
            // =====================================================

            if (model.ProfilePicture != null &&
                model.ProfilePicture.Length > 0)
            {
                var profilePhotoPath = await SaveProfilePictureAsync(
                    model.ProfilePicture,
                    userId,
                    doctorId);

                doctor.ProfilePhoto = profilePhotoPath;
            }


            // =====================================================
            // SAVE DATABASE CHANGES
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // RETURN UPDATED PARTIAL FOR HTMX
            // =====================================================

            var updatedModel = new DoctorProfileViewModel
            {
                UserId = user.UserId,
                DoctorId = doctor.DoctorId,

                // User information
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,

                // Doctor information
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                ProfilePhotoPath = doctor.ProfilePhoto
            };

            return PartialView("_Profile", updatedModel);
        }


        private async Task<string> SaveProfilePictureAsync(
            IFormFile file,
            int userId,
            int doctorId)
        {
            // Get the uploaded file extension
            var extension = Path.GetExtension(file.FileName);

            // Example:
            // userId = 12
            // doctorId = 14
            // result = 1214.png
            var fileName = $"{userId}{doctorId}{extension}";


            // Relative folder inside wwwroot
            var relativeDirectory = Path.Combine(
                "User",
                "Doctor",
                "Profile_Picture");


            // Physical directory
            var physicalDirectory = Path.Combine(
                _environment.WebRootPath,
                relativeDirectory);


            // Create directory if it doesn't exist
            if (!Directory.Exists(physicalDirectory))
            {
                Directory.CreateDirectory(physicalDirectory);
            }


            // Complete physical file path
            var physicalFilePath = Path.Combine(
                physicalDirectory,
                fileName);


            // Save / replace the file
            await using var stream = new FileStream(
                physicalFilePath,
                FileMode.Create);

            await file.CopyToAsync(stream);


            // Path stored in database
            return $"User/Doctor/Profile_Picture/{fileName}";
        }



        //changing the password
        [HttpGet]
        public IActionResult ChangePasswordPartial()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = userId
            };

            return PartialView("_Password", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // Never trust UserId coming from the browser.
            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return PartialView("_ChangePassword", model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);

            if (user == null)
            {
                return NotFound();
            }

            // =====================================================
            // VERIFY CURRENT PASSWORD
            // =====================================================

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(
                    model.OldPassword,
                    user.PasswordHash))
            {
                ModelState.AddModelError(
                    nameof(model.OldPassword),
                    "The current password is incorrect.");

                return PartialView("_ChangePassword", model);
            }


            // =====================================================
            // UPDATE PASSWORD
            // =====================================================

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);


            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["SuccessMessage"] =
                "Your password has been changed successfully.";

            return RedirectToAction(nameof(DoctorProfile));
        }

    }
}