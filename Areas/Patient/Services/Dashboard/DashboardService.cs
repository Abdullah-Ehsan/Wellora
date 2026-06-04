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
            // 1. Fetch core patient profile and cross-reference joined user information
            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null) return null;

            var userAccount = _context.Users.FirstOrDefault(u => u.UserId == userId);
            string username = userAccount?.Username ?? "N/A";
            string email = userAccount?.Email ?? "N/A";

            var dashboard = new PatientDashboardViewModel
            {
                PatientId = patient.PatientId,
                UserId = userId,
                FullName = patient.FullName ?? "Patient",
                Username = username,
                Email = email,
                Gender = patient.Gender ?? "Unspecified",
                Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                ProfilePhoto = patient.ProfilePhoto,

                // Account Detailed Section mapping from patient record
                Address = patient.Address ?? "Not Provided",
                BloodGroup = patient.BloodGroup ?? "N/A",
                EmergencyContactName = patient.EmergencyContactName ?? "Not Set",
                EmergencyContactPhone = patient.EmergencyContactPhone ?? "Not Set",
                Allergies = patient.Allergies ?? "None Known",
                MedicalConditions = patient.MedicalConditions ?? "None Logged",
                Medications = patient.Medications ?? "None",
                PreferredLanguage = patient.PreferredLanguage ?? "English",
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };

            // 2. Fetch top three upcoming active appointments
            dashboard.UpcomingAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.PatientId && a.AppointmentDate >= DateTime.Now && a.Status == "scheduled")
                .OrderBy(a => a.AppointmentDate)
                .Take(3)
                .Select(a => new DashboardAppointmentItem
                {
                    DoctorName = a.Doctor.FullName,
                    Specialization = a.Doctor.Specialization,
                    DoctorPhoto = a.Doctor.ProfilePhoto,
                    AppointmentDate = a.AppointmentDate
                })
                .ToList();

            // 3. Compute Financial Expenditures
            var patientAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.PatientId)
                .ToList();

            if (patientAppointments.Any())
            {
                dashboard.TotalExpenditure = patientAppointments.Sum(a => a.ConsultationFee);

                // Most Visited Doctor Logic
                var mostVisited = patientAppointments
                    .GroupBy(a => a.Doctor.FullName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                dashboard.MostVisitedDoctor = mostVisited != null ? $"Dr. {mostVisited.Key} ({mostVisited.Count()} visits)" : "None";

                // Most Expensive Doctor Logic
                var mostExpensive = patientAppointments
                    .OrderByDescending(a => a.ConsultationFee)
                    .FirstOrDefault();
                dashboard.MostExpensiveDoctor = mostExpensive != null ? $"Dr. {mostExpensive.Doctor.FullName} (Rs. {mostExpensive.ConsultationFee:F2})" : "None";
            }

            return dashboard;
        }
    }
}