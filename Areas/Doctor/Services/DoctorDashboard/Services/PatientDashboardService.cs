using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;
using Wellora.Models;

namespace Wellora.Services.DoctorDashboard.Services
{
    public class PatientDashboardService : IPatientDashboardService
    {
        private readonly ApplicationDbContext _context;

        public PatientDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // MAIN PATIENT STATS WRAPPER
        // =========================================
        public async Task<PatientStatsViewModel> GetPatientStatsAsync(int doctorId)
        {
            var topSpending = await GetTopSpendingPatientsAsync(doctorId, 3);

            var topVisited = await GetMostVisitedPatientsAsync(doctorId, 3);

            return new PatientStatsViewModel
            {
                TopSpendingPatients = topSpending,
                TopVisitedPatients = topVisited
            };
        }

        // =========================================
        // TOP SPENDING PATIENT (SUM OF CONSULTATION FEE)
        // =========================================
        public async Task<List<PatientSummaryViewModel>> GetTopSpendingPatientsAsync(int doctorId, int top = 3)
        {
            var result = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.PaymentStatus == "paid")
                .GroupBy(a => new
                {
                    a.PatientId,
                    a.Patient.FullName,
                    a.Patient.ProfilePhoto
                })
                .Select(g => new PatientSummaryViewModel
                {
                    PatientId = g.Key.PatientId,
                    PatientName = g.Key.FullName,
                    PatientPhoto = g.Key.ProfilePhoto,

                    AppointmentCount = g.Count(),

                    TotalSpent = g.Sum(x => x.ConsultationFee),

                    LastAppointmentDate = g.Max(x => x.AppointmentDate)  
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToListAsync();

            return result;
        }

        // =========================================
        // MOST VISITED PATIENT (COUNT OF APPOINTMENTS)
        // =========================================
        public async Task<List<PatientSummaryViewModel>> GetMostVisitedPatientsAsync(int doctorId, int top = 3)
        {
            var result = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .GroupBy(a => new
                {
                    a.PatientId,
                    a.Patient.FullName,
                    a.Patient.ProfilePhoto
                })
                .Select(g => new PatientSummaryViewModel
                {
                    PatientId = g.Key.PatientId,
                    PatientName = g.Key.FullName,
                    PatientPhoto = g.Key.ProfilePhoto,

                    AppointmentCount = g.Count(),

                    TotalSpent = g.Sum(x => x.ConsultationFee),

                    LastAppointmentDate = g.Max(x => x.AppointmentDate)   
                })
                .OrderByDescending(x => x.AppointmentCount)
                .Take(top)
                .ToListAsync();

            return result;
        }
    }
}