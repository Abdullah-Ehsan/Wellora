using System;

namespace Wellora.Areas.Patient.ViewModels
{
    public class DoctorViewModel
    {
        // Base Info
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? ContactNumber { get; set; }
        public string? HospitalAddress { get; set; }
        public string? Country { get; set; }

        // Professional Credentials
        public string LicenseNumber { get; set; }
        public string? PmdcNumber { get; set; }
        public string? MedicalSchool { get; set; }
        public string? Certifications { get; set; }
        public string? Qualifications { get; set; }

        // Clinical Practice
        public int? YearsExperience { get; set; }
        public string? ClinicHours { get; set; }
        public string? DaysAvailable { get; set; }
        public bool? TelemedicineAvailable { get; set; }
        public int? AppointmentDurationMin { get; set; }
        public string? BreakTimes { get; set; }
        public int? MaxPatientsPerDay { get; set; }
        public decimal ConsultationFee { get; set; }

        // Specialties & Services
        public string Specialization { get; set; }
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
    }
}
