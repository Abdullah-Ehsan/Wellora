namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }


        // =====================================================
        // PROFESSIONAL / SPECIALIZATION INFO
        // =====================================================

        public string? Specialization { get; set; }

        public string? SubSpecialties { get; set; }

        public int? YearsExperience { get; set; }

        public string? ServicesOffered { get; set; }


        // =====================================================
        // EDUCATION
        // =====================================================

        public string? PrimaryMedicalDegree { get; set; }

        public string? PostgraduateDegree { get; set; }

        public string? SuperSpecialty { get; set; }

        public string? ProfessionalCertification { get; set; }

        public string? AdditionalDegree { get; set; }


        // =====================================================
        // NARRATIVE / CAREER HIGHLIGHTS
        // =====================================================

        public string? Biography { get; set; }

        public string? Achievements { get; set; }

        public string? Publications { get; set; }


        // =====================================================
        // COMMUNICATION
        // =====================================================

        public string? LanguagesSpoken { get; set; }

        public string? CountryCode { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ContactNumber { get; set; }


        public string? SocialLinks { get; set; }


        // =====================================================
        // CLINICAL / REGULATORY INFO
        // =====================================================

        public string? HospitalAddress { get; set; }

        public string? LicenseNumber { get; set; }

        public string? PmdcNumber { get; set; }

        public string? Country { get; set; }

        public string? MedicalSchool { get; set; }

        public string? Certifications { get; set; }


        // =====================================================
        // OPTIONAL FLAGS
        // =====================================================

        public bool? TelemedicineAvailable { get; set; }

        public decimal ConsultationFee { get; set; }
    }
}
