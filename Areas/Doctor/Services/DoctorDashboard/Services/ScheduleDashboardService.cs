using Microsoft.EntityFrameworkCore;
using Wellora.Data;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Services
{
    public class ScheduleDashboardService : IScheduleDashboardService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeeklyScheduleRowViewModel>> GetWeeklyScheduleAsync(int doctorId)
        {
            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();

            var breaks = await _context.DoctorBreaks
                .Where(b => b.DoctorId == doctorId)
                .ToListAsync();

            var weekDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

            var result = new List<WeeklyScheduleRowViewModel>();

            foreach (var day in weekDays)
            {
                var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == day);
                var dayBreak = breaks.FirstOrDefault(b => b.DayOfWeek == day);

                if (schedule == null)
                {
                    result.Add(new WeeklyScheduleRowViewModel
                    {
                        DayName = GetDayName(day),
                        WorkingHours = "Day Off",
                        BreakTime = "-",
                        Duration = "-",
                        MaxPatients = "-",
                        IsOffDay = true
                    });

                    continue;
                }

                result.Add(new WeeklyScheduleRowViewModel
                {
                    DayName = GetDayName(day),
                    WorkingHours = $"{FormatTime(schedule.StartTime)} - {FormatTime(schedule.EndTime)}",
                    BreakTime = dayBreak != null
                        ? $"{FormatTime(dayBreak.BreakStart)} - {FormatTime(dayBreak.BreakEnd)}"
                        : "-",
                    Duration = $"{schedule.AppointmentDurationMin} min",
                    MaxPatients = schedule.MaxPatientsPerDay.ToString(),
                    IsOffDay = false
                });
            }

            return result;
        }

        // =========================================
        // HELPERS
        // =========================================

        private string GetDayName(int day)
        {
            return day switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => "-"
            };
        }

        private string FormatTime(TimeSpan time)
        {
            var dateTime = DateTime.Today.Add(time);
            return dateTime.ToString("hh:mm tt");
        }
    }
}