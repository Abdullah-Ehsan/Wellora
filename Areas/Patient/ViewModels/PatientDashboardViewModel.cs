using System;
using System.Collections.Generic;

namespace Wellora.Areas.Patient.ViewModels
{
    public class PatientDashboardViewModel
    {
        // =====================================================
        // TOP SECTION: CORE PROFILE INFORMATION
        // =====================================================

        public int PatientId { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        public int Age { get; set; }

        public string? ProfilePhoto { get; set; }


        // =====================================================
        // METRICS SECTION
        // =====================================================

        public string MostVisitedDoctor { get; set; } = "None";

        public string MostExpensiveDoctor { get; set; } = "None";

        public decimal TotalExpenditure { get; set; }


        // =====================================================
        // UPCOMING APPOINTMENTS
        // =====================================================

        public List<DashboardAppointmentItem> UpcomingAppointments { get; set; }
            = new List<DashboardAppointmentItem>();


        // =====================================================
        // APPOINTMENT VOLUME CHART
        // =====================================================

        public List<string> AppointmentChartLabels { get; set; }
            = new List<string>();

        public List<int> AppointmentChartData { get; set; }
            = new List<int>();


        // =====================================================
        // APPOINTMENT STATUS CHART
        // =====================================================

        public List<string> AppointmentStatusLabels { get; set; }
            = new List<string>();

        public List<int> AppointmentStatusData { get; set; }
            = new List<int>();


        // =====================================================
        // PAYMENT STATUS CHART
        // =====================================================

        public List<string> PaymentStatusLabels { get; set; }
            = new List<string>();

        public List<int> PaymentStatusData { get; set; }
            = new List<int>();

        public int PaidTransactions { get; set; }

        public int PendingTransactions { get; set; }

        public int FailedTransactions { get; set; }

        public int RefundedTransactions { get; set; }


        // =====================================================
        // PAYMENT METHOD CHART
        // =====================================================

        public List<string> PaymentMethodLabels { get; set; }
            = new List<string>();

        public List<int> PaymentMethodData { get; set; }
            = new List<int>();

        public int CardPayments { get; set; }

        public int CashPayments { get; set; }

        public int OnlinePayments { get; set; }


        // =====================================================
        // APPOINTMENT STATUS COUNTS
        // =====================================================

        public int ScheduledAppointments { get; set; }

        public int CheckedAppointments { get; set; }

        public int CancelledAppointments { get; set; }


        // =====================================================
        // DETAILED ACCOUNT METADATA
        // =====================================================

        public string? Address { get; set; }

        public string? BloodGroup { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? Allergies { get; set; }

        public string? MedicalConditions { get; set; }

        public string? Medications { get; set; }

        public string? PreferredLanguage { get; set; }


        // =====================================================
        // TIMELINE FOOTPRINTS
        // =====================================================

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
