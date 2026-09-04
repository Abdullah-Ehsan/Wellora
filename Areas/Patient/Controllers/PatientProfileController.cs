using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Patient.Models;
using Wellora.Areas.Patient.Services.PatientProfile;
using Wellora.Areas.Patient.ViewModels.PatientProfile;
using Wellora.Data;
using Wellora.Models;
using PatientEntity = Wellora.Areas.Patient.Models.Patient;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;



        public PatientProfileController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =========================================================
        // PATIENT PROFILE
        // =========================================================

        [HttpGet]
        public IActionResult PatientProfile()
        {
            return View();
        }


        // =========================================================
        // CURRENT USER ID
        // =========================================================
        //
        // Gets the authenticated application's UserId from:
        //
        // ClaimTypes.NameIdentifier
        //
        // This should be used whenever we need the UserId.
        // =========================================================

        private int GetUserId()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated user ID could not be determined.");
            }

            return userId;
        }


        // =========================================================
        // CURRENT PATIENT ID
        // =========================================================
        //
        // Gets the PatientId from the custom:
        //
        // CurrentPatientId
        //
        // claim created during authentication.
        // =========================================================

        private int GetPatientId()
        {
            var patientIdClaim =
                User.FindFirstValue("CurrentPatientId");

            if (!int.TryParse(
                    patientIdClaim,
                    out var patientId))
            {
                throw new UnauthorizedAccessException(
                    "Authenticated patient ID could not be determined.");
            }

            return patientId;
        }


        // =========================================================
        // CURRENT USER CLAIM VALUE
        // =========================================================
        //
        // Optional helper when we need the actual claim string.
        // =========================================================

        private string? GetCurrentUserName()
        {
            return User.FindFirstValue(
                ClaimTypes.Name);
        }


        // =========================================================
        // CURRENT USER EMAIL
        // =========================================================

        private string? GetCurrentUserEmail()
        {
            return User.FindFirstValue(
                ClaimTypes.Email);
        }

        private async Task<string> SaveProfilePictureAsync(
    IFormFile file,
    int userId,
    int patientId,
    CancellationToken cancellationToken)
        {
            // =========================================================
            // FILE EXTENSION
            // =========================================================

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();


            // =========================================================
            // FILE NAME
            // =========================================================
            //
            // Example:
            //
            // userId    = 12
            // patientId = 14
            //
            // Result:
            //
            // 1214.png
            //
            // =========================================================

            var fileName =
                $"{userId}{patientId}{extension}";


            // =========================================================
            // RELATIVE DIRECTORY
            // =========================================================

            var relativeDirectory =
                Path.Combine(
                    "User",
                    "Patient",
                    "Profile_Picture");


            // =========================================================
            // PHYSICAL DIRECTORY
            // =========================================================

            var physicalDirectory =
                Path.Combine(
                    _environment.WebRootPath,
                    relativeDirectory);


            // =========================================================
            // CREATE DIRECTORY
            // =========================================================

            if (!Directory.Exists(
                    physicalDirectory))
            {
                Directory.CreateDirectory(
                    physicalDirectory);
            }


            // =========================================================
            // PHYSICAL FILE PATH
            // =========================================================

            var physicalFilePath =
                Path.Combine(
                    physicalDirectory,
                    fileName);


            // =========================================================
            // SAVE / REPLACE
            // =========================================================

            await using var stream =
                new FileStream(
                    physicalFilePath,
                    FileMode.Create);


            await file.CopyToAsync(
                stream,
                cancellationToken);


            // =========================================================
            // DATABASE PATH
            // =========================================================

            return $"User/Patient/Profile_Picture/{fileName}";
        }


        // =========================================================
        // RETURN PROFILE PARTIAL WITH VALIDATION ERRORS
        // =========================================================

        private async Task<IActionResult> ReturnProfileInfoWithErrors(
            int patientId,
            int userId,
            ProfileInfoViewModel model,
            CancellationToken cancellationToken)
        {
            // ---------------------------------------------------------
            // Keep the values entered by the user.
            // ---------------------------------------------------------

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);


            if (patient == null || user == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Profile photo should still come from the database
            // when validation fails.
            // ---------------------------------------------------------

            model.ProfilePhoto =
                patient.ProfilePhoto;


            return PartialView(
                "_ProfileInfo",
                model);
        }


        //=======================================================================================
        //Partial Pages

        // =========================================================
        // PROFILE INFORMATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ProfileInfoPartial(
    CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var patient = await _context.Patients
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    p => p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileInfoViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,

                FullName = patient.FullName,

                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Address = patient.Address,
                PreferredLanguage = patient.PreferredLanguage,
                ProfilePhoto = patient.ProfilePhoto,

                Email = user.Email,
                Username = user.Username
            };

            return PartialView("_ProfileInfo", model);
        }



        // =========================================================
        // UPDATE PROFILE INFORMATION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileInfo(
            ProfileInfoViewModel model,
            int DateOfBirthDay,
            int DateOfBirthMonth,
            int DateOfBirthYear,
            IFormFile? profilePhotoFile,
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();
            var userId = GetUserId();


            // ---------------------------------------------------------
            // Validate date
            // ---------------------------------------------------------

            DateOnly dateOfBirth;

            try
            {
                dateOfBirth = new DateOnly(
                    DateOfBirthYear,
                    DateOfBirthMonth,
                    DateOfBirthDay);
            }
            catch
            {
                ModelState.AddModelError(
                    "DateOfBirth",
                    "Please select a valid date of birth.");

                return await ReturnProfileInfoWithErrors(
                    patientId,
                    userId,
                    model,
                    cancellationToken);
            }


            // ---------------------------------------------------------
            // Validate uploaded image
            // ---------------------------------------------------------

            if (profilePhotoFile != null &&
                profilePhotoFile.Length > 0)
            {
                const long maxFileSize =
                    5 * 1024 * 1024;

                if (profilePhotoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "ProfilePhoto",
                        "Profile photo must be smaller than 5 MB.");

                    return await ReturnProfileInfoWithErrors(
                        patientId,
                        userId,
                        model,
                        cancellationToken);
                }


                var extension =
                    Path.GetExtension(
                        profilePhotoFile.FileName)
                    .ToLowerInvariant();


                var allowedExtensions =
                    new[]
                    {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
                    };


                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ProfilePhoto",
                        "Only JPG, PNG and WebP images are allowed.");

                    return await ReturnProfileInfoWithErrors(
                        patientId,
                        userId,
                        model,
                        cancellationToken);
                }
            }


            // ---------------------------------------------------------
            // Validate model
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return await ReturnProfileInfoWithErrors(
                    patientId,
                    userId,
                    model,
                    cancellationToken);
            }


            // ---------------------------------------------------------
            // Get patient
            // ---------------------------------------------------------

            var patient = await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Get user
            // ---------------------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);

            if (user == null)
            {
                return NotFound();
            }


            // =========================================================
            // UPDATE USER ENTITY
            // =========================================================

            user.FirstName =
                model.FirstName?.Trim();

            user.LastName =
                model.LastName?.Trim();

            user.Email =
                model.Email?.Trim();

            user.Username =
                model.Username?.Trim();

            user.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // UPDATE PATIENT ENTITY
            // =========================================================

            patient.FullName = $"{model.FirstName?.Trim()} {model.LastName?.Trim()}".Trim();


            patient.DateOfBirth =
                dateOfBirth;

            patient.Gender =
                model.Gender;

            patient.Address =
                model.Address?.Trim();

            patient.PreferredLanguage =
                model.PreferredLanguage;


            // =========================================================
            // PROFILE PHOTO
            // =========================================================

            if (profilePhotoFile != null &&
                profilePhotoFile.Length > 0)
            {
                var profilePhoto =
                    await SaveProfilePictureAsync(
                        profilePhotoFile,
                        userId,
                        patientId,
                        cancellationToken);

                patient.ProfilePhoto =
                    profilePhoto;
            }


            patient.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync(
                cancellationToken);


            TempData["SuccessMessage"] =
                "Profile information updated successfully.";


            // ---------------------------------------------------------
            // Return the refreshed partial
            // ---------------------------------------------------------
            //
            // Because this is HTMX, we return the partial rather than
            // redirecting to the full PatientProfile page.
            // ---------------------------------------------------------

            var refreshedModel =
                new ProfileInfoViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,

                    FullName = patient.FullName,

                    DateOfBirth = patient.DateOfBirth,

                    Gender = patient.Gender,

                    Address = patient.Address,

                    PreferredLanguage =
                        patient.PreferredLanguage,

                    Email = user.Email,

                    Username = user.Username,

                    ProfilePhoto =
                        patient.ProfilePhoto
                };


            return PartialView(
                "_ProfileInfo",
                refreshedModel);
        }


        // =========================================================
        // CHANGE PASSWORD
        // =========================================================

        [HttpGet]
        public IActionResult ChangePasswordPartial()
        {
            return PartialView(
                "_ChangePassword",
                new ChangePasswordViewModel());
        }

        // =========================================================
        // UPDATE PASSWORD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();


            // ---------------------------------------------------------
            // Validate model
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_ChangePassword",
                    model);
            }


            // ---------------------------------------------------------
            // Get current user
            // ---------------------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);

            if (user == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Verify current password
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(
                    model.OldPassword,
                    user.PasswordHash))
            {
                ModelState.AddModelError(
                    "OldPassword",
                    "The current password is incorrect.");

                return PartialView(
                    "_ChangePassword",
                    model);
            }


            // ---------------------------------------------------------
            // Prevent using the same password
            // ---------------------------------------------------------

            if (BCrypt.Net.BCrypt.Verify(
                    model.NewPassword,
                    user.PasswordHash))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Your new password must be different from your current password.");

                return PartialView(
                    "_ChangePassword",
                    model);
            }


            // ---------------------------------------------------------
            // Hash new password
            // ---------------------------------------------------------

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    model.NewPassword);

            user.UpdatedAt =
                DateTime.UtcNow;


            // ---------------------------------------------------------
            // Save
            // ---------------------------------------------------------

            await _context.SaveChangesAsync(
                cancellationToken);


            // ---------------------------------------------------------
            // Clear password fields
            // ---------------------------------------------------------

            var successModel =
                new ChangePasswordViewModel();


            TempData["SuccessMessage"] =
                "Your password has been updated successfully.";


            // ---------------------------------------------------------
            // Return refreshed partial
            // ---------------------------------------------------------

            return PartialView(
                "_ChangePassword",
                successModel);
        }


        // =========================================================
        // EMERGENCY CONTACTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> EmergencyContactsPartial(
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();
            var userId = GetUserId();

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }

            var model = new EmergencyContactsViewModel
            {
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone
            };

            return PartialView(
                "_EmergencyContacts",
                model);
        }


        // =========================================================
        // UPDATE EMERGENCY CONTACT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmergencyContacts(
            EmergencyContactsViewModel model,
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_EmergencyContacts",
                    model);
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }

            // =========================================================
            // UPDATE EMERGENCY CONTACT
            // =========================================================

            patient.EmergencyContactName =
                model.EmergencyContactName?.Trim();

            patient.EmergencyContactPhone =
                model.EmergencyContactPhone?.Trim();

            patient.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync(
                cancellationToken);


            TempData["SuccessMessage"] =
                "Emergency contact updated successfully.";


            // =========================================================
            // RETURN REFRESHED PARTIAL
            // =========================================================

            var refreshedModel =
                new EmergencyContactsViewModel
                {
                    EmergencyContactName =
                        patient.EmergencyContactName,

                    EmergencyContactPhone =
                        patient.EmergencyContactPhone
                };

            return PartialView(
                "_EmergencyContacts",
                refreshedModel);
        }



        // =========================================================
        // MEDICAL HISTORY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MedicalHistoryPartial(
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();
            var userId = GetUserId();

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }

            var model = new MedicalHistoryViewModel
            {
                Allergies = patient.Allergies,
                MedicalConditions = patient.MedicalConditions,
                Medications = patient.Medications,
                BloodGroup = patient.BloodGroup
            };

            return PartialView(
                "_MedicalHistory",
                model);
        }


        // =========================================================
        // UPDATE MEDICAL HISTORY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMedicalHistory(
            MedicalHistoryViewModel model,
            string? BloodType,
            string? BloodRh,
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();
            var userId = GetUserId();


            // ---------------------------------------------------------
            // Validate blood group selections
            // ---------------------------------------------------------

            var validBloodTypes = new[]
            {
        "A",
        "B",
        "AB",
        "O"
    };

            var validRhValues = new[]
            {
        "+",
        "-"
    };


            if (!string.IsNullOrWhiteSpace(BloodType) &&
                !validBloodTypes.Contains(BloodType))
            {
                ModelState.AddModelError(
                    "BloodGroup",
                    "Please select a valid blood type.");
            }


            if (!string.IsNullOrWhiteSpace(BloodRh) &&
                !validRhValues.Contains(BloodRh))
            {
                ModelState.AddModelError(
                    "BloodGroup",
                    "Please select a valid Rh factor.");
            }


            // ---------------------------------------------------------
            // Validate text lengths
            // ---------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.Allergies) &&
                model.Allergies.Length > 500)
            {
                ModelState.AddModelError(
                    "Allergies",
                    "Allergies information cannot exceed 500 characters.");
            }

            if (!string.IsNullOrWhiteSpace(model.MedicalConditions) &&
                model.MedicalConditions.Length > 500)
            {
                ModelState.AddModelError(
                    "MedicalConditions",
                    "Medical conditions cannot exceed 500 characters.");
            }

            if (!string.IsNullOrWhiteSpace(model.Medications) &&
                model.Medications.Length > 500)
            {
                ModelState.AddModelError(
                    "Medications",
                    "Medications information cannot exceed 500 characters.");
            }


            // ---------------------------------------------------------
            // Return partial if validation fails
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_MedicalHistory",
                    model);
            }


            // ---------------------------------------------------------
            // Get patient
            // ---------------------------------------------------------

            var patient = await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId &&
                         p.UserId == userId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }


            // =========================================================
            // UPDATE MEDICAL HISTORY
            // =========================================================

            patient.Allergies =
                model.Allergies?.Trim();

            patient.MedicalConditions =
                model.MedicalConditions?.Trim();

            patient.Medications =
                model.Medications?.Trim();


            // =========================================================
            // UPDATE BLOOD GROUP
            // =========================================================

            if (!string.IsNullOrWhiteSpace(BloodType) &&
                !string.IsNullOrWhiteSpace(BloodRh))
            {
                patient.BloodGroup =
                    $"{BloodType}{BloodRh}";
            }
            else
            {
                // Allow the patient to clear their blood group.
                patient.BloodGroup = null;
            }


            patient.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync(
                cancellationToken);


            TempData["SuccessMessage"] =
                "Medical history updated successfully.";


            // =========================================================
            // RETURN REFRESHED PARTIAL
            // =========================================================

            var refreshedModel =
                new MedicalHistoryViewModel
                {
                    Allergies =
                        patient.Allergies,

                    MedicalConditions =
                        patient.MedicalConditions,

                    Medications =
                        patient.Medications,

                    BloodGroup =
                        patient.BloodGroup
                };


            return PartialView(
                "_MedicalHistory",
                refreshedModel);
        }




        // =========================================================
        // OUTSIDE DOCTOR INFORMATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> DoctorInfoPartial(
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();

            // ---------------------------------------------------------
            // Find the outside doctor connected to this patient
            // ---------------------------------------------------------

            var patientOutsideDoctor =
                await _context.PatientOutsideDoctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => p.PatientId == patientId,
                        cancellationToken);

            // ---------------------------------------------------------
            // Patient does not have an outside doctor yet
            // ---------------------------------------------------------

            if (patientOutsideDoctor == null)
            {
                return PartialView(
                    "_DoctorInfo",
                    new OutsideDoctorViewModel());
            }

            // ---------------------------------------------------------
            // Get the outside doctor
            // ---------------------------------------------------------

            var outsideDoctor =
                await _context.OutsideDoctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        d => d.OutsideDoctorId ==
                             patientOutsideDoctor.OutsideDoctorId,
                        cancellationToken);

            // ---------------------------------------------------------
            // Relationship exists but doctor was not found
            // ---------------------------------------------------------

            if (outsideDoctor == null)
            {
                return PartialView(
                    "_DoctorInfo",
                    new OutsideDoctorViewModel());
            }

            // ---------------------------------------------------------
            // Fill the ViewModel with existing information
            // ---------------------------------------------------------

            var model =
                new OutsideDoctorViewModel
                {
                    DoctorName =
                        outsideDoctor.DoctorName,

                    DoctorSpecialty =
                        outsideDoctor.DoctorSpecialty,

                    HospitalName =
                        outsideDoctor.HospitalName,

                    HospitalCity =
                        outsideDoctor.HospitalCity,

                    HospitalCountry =
                        outsideDoctor.HospitalCountry,

                    DoctorPhone =
                        outsideDoctor.DoctorPhone,

                    DoctorEmail =
                        outsideDoctor.DoctorEmail,

                    DoctorPhoto =
                        outsideDoctor.DoctorPhoto
                };

            return PartialView(
                "_DoctorInfo",
                model);
        }

        // =========================================================
        // UPDATE / SAVE OUTSIDE DOCTOR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOutsideDoctor(
            OutsideDoctorViewModel model,
            IFormFile? doctorPhotoFile,
            CancellationToken cancellationToken)
        {
            var patientId = GetPatientId();

            // ---------------------------------------------------------
            // Validate model
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return PartialView("_DoctorInfo", model);
            }


            // ---------------------------------------------------------
            // Get patient
            // ---------------------------------------------------------

            var patient = await _context.Patients
                .FirstOrDefaultAsync(
                    p => p.PatientId == patientId,
                    cancellationToken);

            if (patient == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Create Outside Doctor
            // ---------------------------------------------------------

            var outsideDoctor = new OutsideDoctor
            {
                DoctorName = model.DoctorName.Trim(),

                DoctorSpecialty =
                    model.DoctorSpecialty?.Trim(),

                HospitalName =
                    model.HospitalName?.Trim(),

                HospitalCity =
                    model.HospitalCity?.Trim(),

                HospitalCountry =
                    model.HospitalCountry?.Trim(),

                DoctorPhone =
                    model.DoctorPhone?.Trim(),

                DoctorEmail =
                    model.DoctorEmail?.Trim(),

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow
            };


            // ---------------------------------------------------------
            // Save doctor first
            //
            // MySQL generates OutsideDoctorId automatically.
            // ---------------------------------------------------------

            _context.OutsideDoctors.Add(outsideDoctor);

            await _context.SaveChangesAsync(
                cancellationToken);


            // ---------------------------------------------------------
            // Save doctor photo
            // ---------------------------------------------------------

            if (doctorPhotoFile != null &&
                doctorPhotoFile.Length > 0)
            {
                const long maxFileSize =
                    5 * 1024 * 1024;

                if (doctorPhotoFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "DoctorPhoto",
                        "Doctor photo must be smaller than 5 MB.");

                    return PartialView("_DoctorInfo", model);
                }

                var extension =
                    Path.GetExtension(
                        doctorPhotoFile.FileName)
                    .ToLowerInvariant();

                var allowedExtensions =
                    new[]
                    {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
                    };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "DoctorPhoto",
                        "Only JPG, PNG and WebP images are allowed.");

                    return PartialView("_DoctorInfo", model);
                }

                var fileName =
                    $"{patientId}{outsideDoctor.OutsideDoctorId}{extension}";

                var relativeDirectory =
                    Path.Combine(
                        "User",
                        "Patient",
                        "Outside_Doctor_Picture",
                        "Profile_Picture");

                var physicalDirectory =
                    Path.Combine(
                        _environment.WebRootPath,
                        relativeDirectory);

                if (!Directory.Exists(physicalDirectory))
                {
                    Directory.CreateDirectory(
                        physicalDirectory);
                }

                var physicalFilePath =
                    Path.Combine(
                        physicalDirectory,
                        fileName);

                await using var stream =
                    new FileStream(
                        physicalFilePath,
                        FileMode.Create);

                await doctorPhotoFile.CopyToAsync(
                    stream,
                    cancellationToken);

                outsideDoctor.DoctorPhoto =
                    $"User/Patient/Outside_Doctor_Picture/Profile_Picture/{fileName}";

                outsideDoctor.UpdatedAt =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync(
                    cancellationToken);
            }


            // ---------------------------------------------------------
            // Connect patient with outside doctor
            // ---------------------------------------------------------

            var patientOutsideDoctor =
                new PatientOutsideDoctor
                {
                    PatientId = patientId,

                    OutsideDoctorId =
                        outsideDoctor.OutsideDoctorId,

                    RelationshipType = "Primary Doctor",

                    CreatedAt = DateTime.UtcNow,

                    UpdatedAt = DateTime.UtcNow
                };

            _context.PatientOutsideDoctors.Add(
                patientOutsideDoctor);


            // ---------------------------------------------------------
            // Save relationship
            // ---------------------------------------------------------

            await _context.SaveChangesAsync(
                cancellationToken);


            // ---------------------------------------------------------
            // Return refreshed partial
            // ---------------------------------------------------------

            var refreshedModel =
                new OutsideDoctorViewModel
                {
                    DoctorName =
                        outsideDoctor.DoctorName,

                    DoctorSpecialty =
                        outsideDoctor.DoctorSpecialty,

                    HospitalName =
                        outsideDoctor.HospitalName,

                    HospitalCity =
                        outsideDoctor.HospitalCity,

                    HospitalCountry =
                        outsideDoctor.HospitalCountry,

                    DoctorPhone =
                        outsideDoctor.DoctorPhone,

                    DoctorEmail =
                        outsideDoctor.DoctorEmail,

                    DoctorPhoto =
                        outsideDoctor.DoctorPhoto
                };


            return PartialView(
                "_DoctorInfo",
                refreshedModel);
        }

    }
}
