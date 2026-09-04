using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Admin.ViewModels.AdminProfile;
using Wellora.Data;

namespace Wellora.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class AdminProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminProfileController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // ADMIN PROFILE MAIN PAGE
        // =========================================================

        [HttpGet]
        public IActionResult AdminProfile()
        {
            return View();
        }

        // =========================================================
        // CURRENT USER ID
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
        // SAVE ADMIN PROFILE PICTURE
        // =========================================================

        private async Task<string> SaveAdminProfilePictureAsync(
            IFormFile file,
            int userId,
            int adminId,
            CancellationToken cancellationToken)
        {
            // ---------------------------------------------------------
            // Get extension
            // ---------------------------------------------------------

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();


            // ---------------------------------------------------------
            // File name
            //
            // Example:
            //
            // userId = 12
            // adminId = 4
            //
            // 124.png
            // ---------------------------------------------------------

            var fileName =
                $"{userId}{adminId}{extension}";


            // ---------------------------------------------------------
            // Relative directory
            // ---------------------------------------------------------

            var relativeDirectory =
                Path.Combine(
                    "User",
                    "Admin",
                    "Profile_Picture");


            // ---------------------------------------------------------
            // Physical directory
            // ---------------------------------------------------------

            var physicalDirectory =
                Path.Combine(
                    _environment.WebRootPath,
                    relativeDirectory);


            // ---------------------------------------------------------
            // Create directory if necessary
            // ---------------------------------------------------------

            if (!Directory.Exists(
                    physicalDirectory))
            {
                Directory.CreateDirectory(
                    physicalDirectory);
            }


            // ---------------------------------------------------------
            // Physical file path
            // ---------------------------------------------------------

            var physicalFilePath =
                Path.Combine(
                    physicalDirectory,
                    fileName);


            // ---------------------------------------------------------
            // Save / replace image
            // ---------------------------------------------------------

            await using var stream =
                new FileStream(
                    physicalFilePath,
                    FileMode.Create);


            await file.CopyToAsync(
                stream,
                cancellationToken);


            // ---------------------------------------------------------
            // Database path
            // ---------------------------------------------------------

            return
                $"User/Admin/Profile_Picture/{fileName}";
        }


        // =========================================================
        // PROFILE INFO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ProfileInfoPartial(
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();


            // ---------------------------------------------------------
            // Get admin
            // ---------------------------------------------------------

            var admin = await _context.Admins
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    a => a.UserId == userId,
                    cancellationToken);

            if (admin == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Get user
            // ---------------------------------------------------------

            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    u => u.UserId == userId,
                    cancellationToken);

            if (user == null)
            {
                return NotFound();
            }


            // ---------------------------------------------------------
            // Build view model
            // ---------------------------------------------------------

            var model =
                new ProfileInfoViewModel
                {
                    FirstName =
                        user.FirstName,

                    LastName =
                        user.LastName,

                    DateOfBirth =
                        admin.DateOfBirth,

                    Gender =
                        admin.Gender,

                    Address =
                        admin.Address,

                    ProfilePicture =
                        admin.ProfilePicture
                };


            // ---------------------------------------------------------
            // Return HTMX partial
            // ---------------------------------------------------------

            return PartialView(
                "_ProfileInfo",
                model);
        }


        // =========================================================
        // UPDATE PROFILE INFO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileInfo(
            ProfileInfoViewModel model,
            IFormFile? profilePhotoFile,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();


            // ---------------------------------------------------------
            // Validate profile image
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

                    model.ProfilePicture = null;

                    return PartialView(
                        "_ProfileInfo",
                        model);
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


                if (!allowedExtensions.Contains(
                        extension))
                {
                    ModelState.AddModelError(
                        "ProfilePhoto",
                        "Only JPG, PNG and WebP images are allowed.");

                    model.ProfilePicture = null;

                    return PartialView(
                        "_ProfileInfo",
                        model);
                }
            }


            // ---------------------------------------------------------
            // Validate model
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_ProfileInfo",
                    model);
            }


            // ---------------------------------------------------------
            // Get admin
            // ---------------------------------------------------------

            var admin = await _context.Admins
                .FirstOrDefaultAsync(
                    a => a.UserId == userId,
                    cancellationToken);

            if (admin == null)
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
            // UPDATE USER
            // =========================================================

            user.FirstName =
                model.FirstName?.Trim();

            user.LastName =
                model.LastName?.Trim();

            user.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // UPDATE ADMIN
            // =========================================================

            admin.FullName =
                $"{model.FirstName?.Trim()} {model.LastName?.Trim()}"
                    .Trim();

            admin.DateOfBirth =
                model.DateOfBirth;

            admin.Gender =
                model.Gender;

            admin.Address =
                model.Address?.Trim();

            admin.UpdatedAt =
                DateTime.UtcNow;


            // =========================================================
            // PROFILE PICTURE
            // =========================================================

            if (profilePhotoFile != null &&
                profilePhotoFile.Length > 0)
            {
                var profilePicture =
                    await SaveAdminProfilePictureAsync(
                        profilePhotoFile,
                        userId,
                        admin.AdminId,
                        cancellationToken);

                admin.ProfilePicture =
                    profilePicture;
            }


            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync(
                cancellationToken);


            TempData["SuccessMessage"] =
                "Profile information updated successfully.";


            // =========================================================
            // RETURN REFRESHED PARTIAL
            // =========================================================

            var refreshedModel =
                new ProfileInfoViewModel
                {
                    FirstName =
                        user.FirstName,

                    LastName =
                        user.LastName,

                    DateOfBirth =
                        admin.DateOfBirth,

                    Gender =
                        admin.Gender,

                    Address =
                        admin.Address,

                    ProfilePicture =
                        admin.ProfilePicture
                };


            return PartialView(
                "_ProfileInfo",
                refreshedModel);
        }



        // =========================================================
        // CONTACT INFORMATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ContactInfoPartial(
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var admin = await _context.Admins
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    a => a.UserId == userId,
                    cancellationToken);

            if (admin == null)
            {
                return NotFound();
            }

            var model = new ContactInfoViewModel
            {
                ContactNumber = admin.ContactNumber,
                OfficeNumber = admin.OfficeNumber,
                OfficeOfficialNumber = admin.OfficeOfficialNumber,
                EmergencyContactName = admin.EmergencyContactName,
                EmergencyContactNumber = admin.EmergencyContactNumber
            };

            return PartialView(
                "_ContactInfo",
                model);
        }


        // =========================================================
        // UPDATE CONTACT INFORMATION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(
            ContactInfoViewModel model,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            // ---------------------------------------------------------
            // Validate model
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_ContactInfo",
                    model);
            }

            // ---------------------------------------------------------
            // Get admin
            // ---------------------------------------------------------

            var admin = await _context.Admins
                .FirstOrDefaultAsync(
                    a => a.UserId == userId,
                    cancellationToken);

            if (admin == null)
            {
                return NotFound();
            }

            // =========================================================
            // UPDATE CONTACT INFORMATION
            // =========================================================

            admin.ContactNumber =
                model.ContactNumber?.Trim();

            admin.OfficeNumber =
                model.OfficeNumber?.Trim();

            admin.OfficeOfficialNumber =
                model.OfficeOfficialNumber?.Trim();

            admin.EmergencyContactName =
                model.EmergencyContactName?.Trim();

            admin.EmergencyContactNumber =
                model.EmergencyContactNumber?.Trim();

            admin.UpdatedAt =
                DateTime.UtcNow;

            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync(
                cancellationToken);

            TempData["SuccessMessage"] =
                "Contact information updated successfully.";

            // =========================================================
            // RETURN REFRESHED PARTIAL
            // =========================================================

            var refreshedModel =
                new ContactInfoViewModel
                {
                    ContactNumber =
                        admin.ContactNumber,

                    OfficeNumber =
                        admin.OfficeNumber,

                    OfficeOfficialNumber =
                        admin.OfficeOfficialNumber,

                    EmergencyContactName =
                        admin.EmergencyContactName,

                    EmergencyContactNumber =
                        admin.EmergencyContactNumber
                };

            return PartialView(
                "_ContactInfo",
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
            // Make sure the account has a password
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError(
                    "OldPassword",
                    "Your current password could not be verified.");

                return PartialView(
                    "_ChangePassword",
                    model);
            }


            // ---------------------------------------------------------
            // Verify current password
            // ---------------------------------------------------------

            if (!BCrypt.Net.BCrypt.Verify(
                    model.CurrentPassword,
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
            // Validate password requirements
            // ---------------------------------------------------------

            if (model.NewPassword.Length < 8)
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Password must be at least 8 characters long.");
            }

            if (!model.NewPassword.Any(char.IsUpper))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Password must contain at least one uppercase letter.");
            }

            if (!model.NewPassword.Any(char.IsDigit))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Password must contain at least one number.");
            }

            if (!model.NewPassword.Any(
                    ch => !char.IsLetterOrDigit(ch)))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Password must contain at least one special character.");
            }


            // ---------------------------------------------------------
            // Return validation errors
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
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


            // ---------------------------------------------------------
            // Update timestamp
            // ---------------------------------------------------------

            user.UpdatedAt =
                DateTime.UtcNow;


            // ---------------------------------------------------------
            // Save
            // ---------------------------------------------------------

            await _context.SaveChangesAsync(
                cancellationToken);


            // ---------------------------------------------------------
            // Success
            // ---------------------------------------------------------

            TempData["SuccessMessage"] =
                "Your password has been updated successfully.";


            // ---------------------------------------------------------
            // Return fresh empty form
            // ---------------------------------------------------------

            return PartialView(
                "_ChangePassword",
                new ChangePasswordViewModel());
        }

    }
}
