using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wellora.Areas.Patient.Services.PatientProfile;
using Wellora.Areas.Patient.ViewModels.PatientProfile;
using Wellora.Models;
using PatientEntity = Wellora.Areas.Patient.Models.Patient;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientProfileController : Controller
    {
        private readonly PatientProfileService _service;

        public PatientProfileController(PatientProfileService service)
        {
            _service = service;
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var id) ? id : 0;
        }

        // Main page
        public IActionResult PatientProfile()
        {
            return View("PatientProfile");
        }

        // Profile Info
        public IActionResult ProfileInfo()
        {
            int userId = GetLoggedInUserId();
            var patient = _service.GetPatientByUserId(userId);
            var user = _service.GetUserById(userId);

            var vm = new ProfileInfoViewModel
            {
                FullName = patient?.FullName ?? string.Empty,
                DateOfBirth = patient?.DateOfBirth ?? DateOnly.MinValue,
                Gender = patient?.Gender ?? string.Empty,
                Address = patient?.Address ?? string.Empty,
                PreferredLanguage = patient?.PreferredLanguage ?? string.Empty,
                Email = user?.Email ?? string.Empty,      // from User table
                Username = user?.Username ?? string.Empty,
                ProfilePhoto = patient?.ProfilePhoto ?? string.Empty
            };

            return PartialView("_ProfileInfo", vm);
        }

        // Medical History
        public IActionResult MedicalHistory()
        {
            int userId = GetLoggedInUserId();
            var patient = _service.GetPatientByUserId(userId);

            var vm = new MedicalHistoryViewModel
            {
                Allergies = patient?.Allergies,
                MedicalConditions = patient?.MedicalConditions,
                Medications = patient?.Medications,
                BloodGroup = patient?.BloodGroup
            };

            return PartialView("_MedicalHistory", vm);
        }

        [HttpPost]
        public IActionResult UpdateMedicalHistory(MedicalHistoryViewModel vm)
        {
            int userId = GetLoggedInUserId();
            _service.UpdateMedicalHistory(userId, vm);

            return PartialView("_MedicalHistory", vm);
        }

        // Emergency Contacts
        public IActionResult EmergencyContacts()
        {
            int userId = GetLoggedInUserId();
            var patient = _service.GetPatientByUserId(userId);

            var vm = new EmergencyContactsViewModel
            {
                EmergencyContactName = patient?.EmergencyContactName,
                EmergencyContactPhone = patient?.EmergencyContactPhone
            };

            return PartialView("_EmergencyContacts", vm);
        }

        [HttpPost]
        public IActionResult UpdateEmergencyContacts(EmergencyContactsViewModel vm)
        {
            int userId = GetLoggedInUserId();
            _service.UpdateEmergencyContacts(userId, vm);

            return PartialView("_EmergencyContacts", vm);
        }

        // Change Password
        public IActionResult ChangePassword()
        {
            return PartialView("_ChangePassword", new ChangePasswordViewModel());
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel vm)
        {
            int userId = GetLoggedInUserId();
            var user = _service.GetUserById(userId);

            if (user == null) return BadRequest("User not found");

            // Use ASP.NET Core PasswordHasher instead of PasswordHelper
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", vm.OldPassword);

            if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                ModelState.AddModelError("", "Old password incorrect");

            if (vm.NewPassword != vm.ConfirmPassword)
                ModelState.AddModelError("", "Passwords do not match");

            if (!ModelState.IsValid)
                return PartialView("_ChangePassword", vm);

            user.PasswordHash = hasher.HashPassword(user, vm.NewPassword);
            _service.SaveChanges();

            return PartialView("_ChangePassword", new ChangePasswordViewModel());
        }


        public IActionResult DoctorInfo()
        {
            return PartialView("_DoctorInfo");
        }

    }
}
