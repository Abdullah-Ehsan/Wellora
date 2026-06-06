using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Services
{
    public class RevenueDashboardService : IRevenueDashboardService
    {
        private readonly ApplicationDbContext _context;

        public RevenueDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // MAIN REVENUE WRAPPER
        // =========================================
        public async Task<RevenueViewModel> GetRevenueAsync(int doctorId)
        {
            var completedStatus = "checked";

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();

            var today = DateTime.Today;

            var todayRevenue = appointments
                .Where(a => a.AppointmentDate.Date == today && a.Status == completedStatus)
                .Sum(a => a.ConsultationFee);

            var monthlyRevenue = appointments
                .Where(a => a.AppointmentDate.Month == today.Month
                         && a.AppointmentDate.Year == today.Year
                         && a.Status == completedStatus)
                .Sum(a => a.ConsultationFee);

            var totalRevenue = appointments
                .Where(a => a.Status == completedStatus)
                .Sum(a => a.ConsultationFee);

            var completedAppointments = appointments
                .Count(a => a.Status == completedStatus);

            return new RevenueViewModel
            {
                TodayRevenue = todayRevenue,
                MonthlyRevenue = monthlyRevenue,
                TotalRevenue = totalRevenue,
                CompletedAppointments = completedAppointments
            };
        }
    }
}