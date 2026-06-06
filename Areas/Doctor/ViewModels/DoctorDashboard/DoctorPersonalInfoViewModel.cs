namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class DoctorPersonalInfoViewModel
    {
        public DateOnly DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Country { get; set; }

        public string? ContactNumber { get; set; }

        public string? HospitalAddress { get; set; }

        public string? LanguagesSpoken { get; set; }
    }
}