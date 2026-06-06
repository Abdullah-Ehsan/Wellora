using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Services
{
    public class GraphDashboardService : IGraphDashboardService
    {
        private readonly ApplicationDbContext _context;

        public GraphDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // MAIN WRAPPER
        // =========================================
        public async Task<GraphDataViewModel> GetGraphDataAsync(int doctorId)
        {
            return new GraphDataViewModel
            {
                MonthlyVisitLabels = await GetMonthlyVisitLabels(doctorId),
                MonthlyVisitValues = await GetMonthlyVisitValues(doctorId),

                WeeklyVisitLabels = await GetWeeklyVisitLabels(doctorId),
                WeeklyVisitValues = await GetWeeklyVisitValues(doctorId),

                RevenueLabels = await GetYearlyRevenueLabels(),
                RevenueValues = await GetYearlyRevenueValues(doctorId)
            };
        }

        // =========================================
        // MONTHLY VISITS (LAST 4 MONTHS)
        // =========================================
        private async Task<List<string>> GetMonthlyVisitLabels(int doctorId)
        {
            var today = DateTime.Today;
            var months = new List<string>();

            for (int i = 3; i >= 0; i--)
            {
                var date = today.AddMonths(-i);
                months.Add(date.ToString("MMM"));
            }

            return months;
        }

        private async Task<List<int>> GetMonthlyVisitValues(int doctorId)
        {
            var today = DateTime.Today;
            var values = new List<int>();

            for (int i = 3; i >= 0; i--)
            {
                var date = today.AddMonths(-i);

                var count = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate.Month == date.Month &&
                        a.AppointmentDate.Year == date.Year)
                    .CountAsync();

                values.Add(count);
            }

            return values;
        }

        // =========================================
        // WEEKLY VISITS (LAST 7 DAYS ENDING TODAY)
        // =========================================
        private async Task<List<string>> GetWeeklyVisitLabels(int doctorId)
        {
            var labels = new List<string>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                labels.Add(date.ToString("ddd")); // Mon, Tue, etc.
            }

            return labels;
        }

        private async Task<List<int>> GetWeeklyVisitValues(int doctorId)
        {
            var values = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);

                var count = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate.Date == date.Date)
                    .CountAsync();

                values.Add(count);
            }

            return values;
        }

        // =========================================
        // YEARLY REVENUE (12 MONTHS)
        // =========================================
        private async Task<List<string>> GetYearlyRevenueLabels()
        {
            var months = new List<string>();

            for (int i = 0; i < 12; i++)
            {
                var date = new DateTime(DateTime.Today.Year, i + 1, 1);
                months.Add(date.ToString("MMM"));
            }

            return months;
        }

        private async Task<List<decimal>> GetYearlyRevenueValues(int doctorId)
        {
            var values = new List<decimal>();
            var completedStatus = "checked";

            for (int i = 1; i <= 12; i++)
            {
                var sum = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate.Month == i &&
                        a.AppointmentDate.Year == DateTime.Today.Year &&
                        a.Status == completedStatus)
                    .SumAsync(a => (decimal?)a.ConsultationFee) ?? 0;

                values.Add(sum);
            }

            return values;
        }
    }
}