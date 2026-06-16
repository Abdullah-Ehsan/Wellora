namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }

        // Core professional info
        public string? Specialization { get; set; }
        public string? SubSpecialties { get; set; }
        public int? YearsExperience { get; set; }
        public string? Qualifications { get; set; }
        public string? ServicesOffered { get; set; }

        // Narrative / career highlights
        public string? Biography { get; set; }
        public string? Achievements { get; set; }
        public string? Publications { get; set; }

        // Communication
        public string? LanguagesSpoken { get; set; }
        public string? ContactNumber { get; set; }
        public string? SocialLinks { get; set; }

        // Clinical / regulatory info
        public string? HospitalAddress { get; set; }
        public string? LicenseNumber { get; set; }
        public string? PmdcNumber { get; set; }
        public string? Country { get; set; }
        public string? MedicalSchool { get; set; }
        public string? Certifications { get; set; }

        // Optional flags
        public bool? TelemedicineAvailable { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}
