using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wellora.Areas.Doctor.Services;
using Wellora.Areas.Doctor.ViewModels.DoctorProfile;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class DoctorProfileController : Controller
    {
        private readonly IDoctorProfileService _doctorProfileService;

        public DoctorProfileController(IDoctorProfileService doctorProfileService)
        {
            _doctorProfileService = doctorProfileService;
        }
        private int GetDoctorId()
        {
            var claim = User.FindFirst("CurrentDoctorId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("CurrentDoctorId claim missing");

            return int.Parse(claim);
        }
        // =========================
        // MAIN PAGE
        // =========================
        public IActionResult DoctorProfile()
        {
            return View("~/Areas/Doctor/Views/DoctorProfile/DoctorProfile.cshtml");
        }

        // =========================
        // HELPER: CLAIMS
        // =========================
        private (int userId, int doctorId) GetIds()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var doctorIdClaim = User.FindFirst("CurrentDoctorId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(doctorIdClaim))
                throw new Exception("Missing authentication claims");

            return (int.Parse(userIdClaim), int.Parse(doctorIdClaim));
        }

        // =========================
        // PROFILE PARTIAL
        // =========================
        public IActionResult ProfilePartial()
        {
            var doctorId = GetDoctorId();

            var model = new DoctorProfileViewModel
            {
                DoctorId = doctorId
            };

            return PartialView("_Profile", model);
        }

        // =========================
        // PASSWORD PARTIAL
        // =========================
        public IActionResult PasswordPartial()
        {
            return PartialView("_Password", new ChangePasswordViewModel());
        }

        // =========================
        // SCHEDULE PARTIAL
        // =========================
        public IActionResult SchedulePartial()
        {
            var doctorId = GetDoctorId();

            var model = new DoctorScheduleUpdateViewModel
            {
                DoctorId = doctorId,
                ScheduleRows = new List<DoctorScheduleRow>(),
                Breaks = new List<DoctorBreakViewModel>()
            };

            return PartialView("_Schedule", model);
        }

        // =========================
        // SPECIALIZATION PARTIAL
        // =========================
        public IActionResult SpecializationPartial()
        {
            var (_, doctorId) = GetIds();

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctorId
            };

            return PartialView("_Specialization", model);
        }

        // =========================
        // CONTACT PARTIAL
        // =========================
        public IActionResult ContactPartial()
        {
            var (_, doctorId) = GetIds();

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctorId
            };

            return PartialView("_Contact", model);
        }

        // =========================
        // CLINICAL INFO PARTIAL
        // =========================
        public IActionResult ClinicalInfoPartial()
        {
            var doctorId = GetDoctorId();

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctorId,
                ServicesOffered = "",
                LanguagesSpoken = "",
                HospitalAddress = "",
                MedicalSchool = "",
                Certifications = ""
            };

            return PartialView("_ClinicalInfo", model);
        }

        // =========================
        // SETTINGS PARTIAL
        // =========================
        public IActionResult SettingsPartial()
        {
            return PartialView("_Settings");
        }

        // =========================
        // ACHIEVEMENTS PARTIAL
        // =========================
        public IActionResult AchievementsPartial()
        {
            var (_, doctorId) = GetIds();

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctorId
            };

            return PartialView("_Achievements", model);
        }

        // =====================================================
        // POST ACTIONS (SERVICE LAYER USED HERE)
        // =====================================================

        [HttpPost]
        public IActionResult UpdateProfile(DoctorProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateProfile(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdatePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdatePassword(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateSpecialization(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateSpecialization(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateContactInfo(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateContactInfo(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateBiography(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateBiography(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateConsultationInfo(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateConsultationInfo(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateDoctorDetails(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateDoctorDetails(model);
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateSocialLinks(DoctorDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _doctorProfileService.UpdateSocialLinks(model);
            return Ok();
        }
    }
}