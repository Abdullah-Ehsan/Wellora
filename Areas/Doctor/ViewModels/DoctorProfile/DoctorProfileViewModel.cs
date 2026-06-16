namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorProfileViewModel
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }

        // Profile photo path (relative to wwwroot)
        public string? ProfilePhotoPath { get; set; }
        public IFormFile? ProfilePicture { get; set; }

        // User info
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Gender { get; set; }

        public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        // Derived property
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Flag for new vs existing profile
        public bool IsNewProfile => string.IsNullOrEmpty(ProfilePhotoPath) && string.IsNullOrEmpty(FirstName);
    }

}
