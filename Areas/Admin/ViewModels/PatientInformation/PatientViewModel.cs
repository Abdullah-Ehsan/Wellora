using System;

namespace Wellora.Areas.Admin.ViewModels.PatientInformation
{
    public class PatientViewModel
    {
        // =========================
        // Basic Information
        // =========================

        public int PatientId { get; set; }

        public int? UserId { get; set; }

        public string? FullName { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? ProfilePhoto { get; set; }


        // =========================
        // Emergency Information
        // =========================

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }


        // =========================
        // Medical Information
        // =========================

        public string? BloodGroup { get; set; }

        public string? Allergies { get; set; }

        public string? MedicalConditions { get; set; }

        public string? Medications { get; set; }


        // =========================
        // Doctor Information
        // =========================

        public int? PrimaryDoctorId { get; set; }


        // =========================
        // Preferences
        // =========================

        public string? PreferredLanguage { get; set; }


        // =========================
        // User Account Information
        // =========================

        public string? Email { get; set; }

        public string? Username { get; set; }

        public string? AccountSituation { get; set; }


        // =========================
        // Dates
        // =========================

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
