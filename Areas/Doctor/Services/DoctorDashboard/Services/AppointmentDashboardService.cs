using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;
using Wellora.Services.DoctorDashboard.Contracts;

namespace Wellora.Services.DoctorDashboard.Services
{
    public class AppointmentDashboardService : IAppointmentDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // TODAY APPOINTMENTS (MAX 10)
        // =========================================
        public async Task<List<TodayAppointmentViewModel>> GetTodayAppointmentsAsync(int doctorId)
        {
            var today = DateTime.Today;

            var data = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today)
                .Select(a => new TodayAppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FullName,
                    PatientPhoto = a.Patient.ProfilePhoto,
                    Allergies = a.Patient.Allergies,
                    AppointmentDate = a.AppointmentDate
                })
                .ToListAsync();   // 🔥 MUST BE FINAL

            return data;
        }

        // =========================================
        // RECENT APPOINTMENTS (FOR WEEKLY GRAPH / STATS)
        // =========================================
        public async Task<List<TodayAppointmentViewModel>> GetRecentAppointmentsAsync(int doctorId, int days = 7)
        {
            var fromDate = DateTime.Today.AddDays(-days);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate >= fromDate)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new TodayAppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FullName,
                    PatientPhoto = a.Patient.ProfilePhoto,
                    Allergies = a.Patient.Allergies,
                    AppointmentDate = a.AppointmentDate
                })
                .ToListAsync();

            return appointments;
        }
    }
}