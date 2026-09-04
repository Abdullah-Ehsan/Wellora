namespace Wellora.Areas.Patient.ViewModels.PatientProfile
{
    public class ProfileInfoViewModel
    {   
        public string? FirstName { get; set; }
        public string? LastName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? ProfilePhoto { get; set; }
    }
}
