using System;

namespace Wellora.Areas.Admin.ViewModels.DoctorInformation
{
    public class DoctorViewModel
    {
        // Base Info
        public int DoctorId { get; set; }
        public string? FullName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? ContactNumber { get; set; }
        public string? HospitalAddress { get; set; }
        public string? Country { get; set; }

        // Professional Credentials
        public string? LicenseNumber { get; set; }
        public string? PmdcNumber { get; set; }
        public string? MedicalSchool { get; set; }
        public string? Certifications { get; set; }
        public string? Qualifications { get; set; }

        public string? PrimaryMedicalDegree { get; set; }
        public string? PostgraduateDegree { get; set; }
        public string? SuperSpecialty { get; set; }
        public string? ProfessionalCertification { get; set; }
        public string? AdditionalDegree { get; set; }

        // Clinical Practice
        public int? YearsExperience { get; set; }
        
        public bool? TelemedicineAvailable { get; set; }
        
        public decimal ConsultationFee { get; set; }

        // Specialties & Services
        public string? Specialization { get; set; }
        public string? SubSpecialties { get; set; }
        public string? ServicesOffered { get; set; }
        public string? LanguagesSpoken { get; set; }

        // Biography & Achievements
        public string? Biography { get; set; }
        public string? Achievements { get; set; }
        public string? Publications { get; set; }

        // Additional Info
        public string? SocialLinks { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public bool? DoctorAvailable { get; set; }

        public string? AccountSituation { get; set; }

        public bool IsPrimaryDoctor { get; set; }

        public List<DoctorScheduleViewModel>? Schedules { get; set; }

    }
}
