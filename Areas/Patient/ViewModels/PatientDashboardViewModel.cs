using System;
using System.Collections.Generic;

namespace Wellora.Areas.Patient.ViewModels
{
    public class PatientDashboardViewModel
    {
        // Top Section: Core Profile Information
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? ProfilePhoto { get; set; }

        // Metrics Section
        public string MostVisitedDoctor { get; set; } = "None";
        public string MostExpensiveDoctor { get; set; } = "None";
        public decimal TotalExpenditure { get; set; }

        // Core List Collection
        public List<DashboardAppointmentItem> UpcomingAppointments { get; set; } = new List<DashboardAppointmentItem>();

        // Detailed Account Metadata Section (All remaining fields from patient table)
        public string? Address { get; set; }
        public string? BloodGroup { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Allergies { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Medications { get; set; }
        public string? PreferredLanguage { get; set; }

        // Timeline Footprints
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DashboardAppointmentItem
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? DoctorPhoto { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}