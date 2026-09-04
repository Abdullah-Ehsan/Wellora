using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Areas.Patient.ViewModels;

namespace Wellora.Services.Dashboard
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }


        public PatientDashboardViewModel GetPatientDashboardData(int userId)
        {
            // =====================================================
            // 1. PATIENT
            // =====================================================

            var patient = _context.Patients
                .AsNoTracking()
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return null;


            // =====================================================
            // 2. USER ACCOUNT
            // =====================================================

            var userAccount = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == userId);


            // =====================================================
            // 3. AGE CALCULATION
            // =====================================================

            var age = CalculateAge(patient.DateOfBirth);


            // =====================================================
            // 4. INITIAL DASHBOARD MODEL
            // =====================================================

            var dashboard = new PatientDashboardViewModel
            {
                PatientId = patient.PatientId,

                UserId = userId,

                FullName = patient.FullName ?? "Patient",

                Username = userAccount?.Username ?? "N/A",

                Email = userAccount?.Email ?? "N/A",

                Gender = patient.Gender ?? "Unspecified",

                Age = age,

                ProfilePhoto = patient.ProfilePhoto,

                Address = patient.Address ?? "Not Provided",

                BloodGroup = patient.BloodGroup ?? "N/A",

                EmergencyContactName =
                    patient.EmergencyContactName ?? "Not Set",

                EmergencyContactPhone =
                    patient.EmergencyContactPhone ?? "Not Set",

                Allergies =
                    patient.Allergies ?? "None Known",

                MedicalConditions =
                    patient.MedicalConditions ?? "None Logged",

                Medications =
                    patient.Medications ?? "None",

                PreferredLanguage =
                    patient.PreferredLanguage ?? "English",

                CreatedAt = patient.CreatedAt,

                UpdatedAt = patient.UpdatedAt
            };


            // =====================================================
            // 5. PATIENT APPOINTMENTS
            // =====================================================

            var patientAppointments = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.PatientId)
                .ToList();


            // =====================================================
            // 6. UPCOMING APPOINTMENTS
            // =====================================================

            dashboard.UpcomingAppointments = patientAppointments
                .Where(a =>
                    a.AppointmentDate >= DateTime.Now &&
                    a.Status == "scheduled")
                .OrderBy(a => a.AppointmentDate)
                .Take(3)
                .Select(a => new DashboardAppointmentItem
                {
                    DoctorName =
                        a.Doctor?.FullName ?? "Unknown Doctor",

                    Specialization =
                        a.Doctor?.Specialization ?? "General",

                    DoctorPhoto =
                        a.Doctor?.ProfilePhoto,

                    AppointmentDate =
                        a.AppointmentDate
                })
                .ToList();


            // =====================================================
            // 7. APPOINTMENT VOLUME - LAST 12 MONTHS
            // =====================================================

            var today = DateTime.Today;

            var currentMonth = new DateTime(
                today.Year,
                today.Month,
                1
            );

            var firstMonth = currentMonth.AddMonths(-11);

            var appointmentGroups = patientAppointments
                .Where(a =>
                    a.AppointmentDate >= firstMonth &&
                    a.AppointmentDate < currentMonth.AddMonths(1))
                .GroupBy(a => new
                {
                    a.AppointmentDate.Year,
                    a.AppointmentDate.Month
                })
                .ToDictionary(
                    g => new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        1),
                    g => g.Count()
                );


            for (int i = 0; i < 12; i++)
            {
                var month = firstMonth.AddMonths(i);

                dashboard.AppointmentChartLabels.Add(
                    month.ToString("MMM")
                );

                dashboard.AppointmentChartData.Add(
                    appointmentGroups.TryGetValue(
                        month,
                        out var count)
                        ? count
                        : 0
                );
            }


            // =====================================================
            // 8. MOST VISITED DOCTOR
            // =====================================================

            var mostVisited = patientAppointments
                .Where(a => a.Doctor != null)
                .GroupBy(a => a.Doctor!.FullName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (mostVisited != null)
            {
                dashboard.MostVisitedDoctor =
                    $"Dr. {mostVisited.Key} ({mostVisited.Count()} visits)";
            }


            // =====================================================
            // 9. MOST EXPENSIVE CONSULTATION
            // =====================================================

            var mostExpensive = patientAppointments
                .Where(a => a.Doctor != null)
                .OrderByDescending(a => a.ConsultationFee)
                .FirstOrDefault();

            if (mostExpensive != null)
            {
                dashboard.MostExpensiveDoctor =
                    $"Dr. {mostExpensive.Doctor!.FullName} " +
                    $"(Rs. {mostExpensive.ConsultationFee:F2})";
            }


            // =====================================================
            // 10. APPOINTMENT STATUS COUNTS
            // =====================================================

            dashboard.ScheduledAppointments =
                patientAppointments.Count(a =>
                    a.Status == "scheduled");

            dashboard.CheckedAppointments =
                patientAppointments.Count(a =>
                    a.Status == "checked");

            dashboard.CancelledAppointments =
                patientAppointments.Count(a =>
                    a.Status == "cancelled");


            // =====================================================
            // 11. APPOINTMENT STATUS CHART DATA
            // =====================================================

            dashboard.AppointmentStatusLabels = new List<string>
            {
                "Scheduled",
                "Checked",
                "Cancelled"
            };

            dashboard.AppointmentStatusData = new List<int>
            {
                dashboard.ScheduledAppointments,
                dashboard.CheckedAppointments,
                dashboard.CancelledAppointments
            };


            // =====================================================
            // 12. TRANSACTIONS
            // =====================================================

            var patientTransactions = _context.Transactions
                .AsNoTracking()
                .Where(t => t.PatientId == patient.PatientId)
                .ToList();


            // =====================================================
            // 13. PAYMENT STATUS COUNTS
            // =====================================================

            dashboard.PaidTransactions =
                patientTransactions.Count(t =>
                    t.Status == "paid");

            dashboard.PendingTransactions =
                patientTransactions.Count(t =>
                    t.Status == "pending");

            dashboard.FailedTransactions =
                patientTransactions.Count(t =>
                    t.Status == "failed");

            dashboard.RefundedTransactions =
                patientTransactions.Count(t =>
                    t.Status == "refunded");


            // =====================================================
            // 14. PAYMENT STATUS CHART DATA
            // =====================================================

            dashboard.PaymentStatusLabels = new List<string>
            {
                "Paid",
                "Pending",
                "Failed",
                "Refunded"
            };

            dashboard.PaymentStatusData = new List<int>
            {
                dashboard.PaidTransactions,
                dashboard.PendingTransactions,
                dashboard.FailedTransactions,
                dashboard.RefundedTransactions
            };


            // =====================================================
            // 15. PAYMENT METHOD COUNTS
            // =====================================================

            dashboard.CardPayments =
                patientTransactions.Count(t =>
                    t.PaymentMethod == "card");

            dashboard.CashPayments =
                patientTransactions.Count(t =>
                    t.PaymentMethod == "cash");

            dashboard.OnlinePayments =
                patientTransactions.Count(t =>
                    t.PaymentMethod == "online");


            // =====================================================
            // 16. PAYMENT METHOD CHART DATA
            // =====================================================

            dashboard.PaymentMethodLabels = new List<string>
            {
                "Card",
                "Cash",
                "Online"
            };

            dashboard.PaymentMethodData = new List<int>
            {
                dashboard.CardPayments,
                dashboard.CashPayments,
                dashboard.OnlinePayments
            };


            // =====================================================
            // 17. ACTUAL EXPENDITURE
            // =====================================================

            dashboard.TotalExpenditure =
                patientTransactions
                    .Where(t => t.Status == "paid")
                    .Sum(t => t.Amount);


            // =====================================================
            // 18. RETURN DASHBOARD
            // =====================================================

            return dashboard;
        }


        // =====================================================
        // AGE CALCULATION
        // =====================================================

        private static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
