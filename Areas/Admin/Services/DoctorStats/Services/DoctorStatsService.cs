using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Admin.Services.DoctorStats.Interfaces;
using Wellora.Areas.Admin.ViewModels.DoctorStats;
using Wellora.Data;

namespace Wellora.Areas.Admin.Services.DoctorStats.Services;

public class DoctorStatsService : IDoctorStatsService
{
    private readonly ApplicationDbContext _context;

    public DoctorStatsService(ApplicationDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // MAIN
    // =========================================================

    public async Task<DoctorStatsViewModel?> GetDoctorStatsAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null)
        {
            return null;
        }

        return new DoctorStatsViewModel
        {
            DoctorId = doctor.DoctorId,
            DoctorName = doctor.FullName ?? "Unknown",
            Specialization = doctor.Specialization ?? "Unknown",
            ProfilePhoto = doctor.ProfilePhoto,

            YearsExperience = doctor.YearsExperience,
            ConsultationFee = doctor.ConsultationFee,


            Summary = await GetSummaryAsync(doctorId),

            Financial = await GetFinancialAnalyticsAsync(doctorId),

            Appointments = await GetAppointmentAnalyticsAsync(doctorId),

            Patients = await GetPatientAnalyticsAsync(doctorId),

            Upcoming = await GetUpcomingAnalyticsAsync(doctorId)
        };
    }


    // =========================================================
    // SUMMARY
    // =========================================================

    private async Task<DoctorStatsSummaryViewModel> GetSummaryAsync(int doctorId)
    {
        var today = DateTime.Today;

        var totalAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a => a.DoctorId == doctorId);

        var completedAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a =>
                a.DoctorId == doctorId &&
                a.Status == "checked");

        var cancelledAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a =>
                a.DoctorId == doctorId &&
                a.Status == "cancelled");

        var upcomingAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate >= today &&
                a.Status != "cancelled");

        var totalPatients = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        var totalEarnings = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var onlineEarnings = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.PaymentMethod == "online" &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var onsiteEarnings = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.PaymentMethod == "cash" &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var onlineAppointments = await _context.Appointments
    .AsNoTracking()
    .CountAsync(a =>
        a.DoctorId == doctorId &&
        a.PaymentMethod == "online");

        var onsiteAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a =>
                a.DoctorId == doctorId &&
                a.PaymentMethod == "cash");


        var averageConsultationFee = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .AverageAsync(a => (decimal?)a.ConsultationFee) ?? 0m;

        var averageRevenuePerAppointment = totalAppointments > 0
            ? totalEarnings / totalAppointments
            : 0m;

        var completionRate = totalAppointments > 0
            ? (decimal)completedAppointments / totalAppointments * 100
            : 0m;

        var cancellationRate = totalAppointments > 0
            ? (decimal)cancelledAppointments / totalAppointments * 100
            : 0m;

        var firstAppointmentDate = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Select(a => (DateTime?)a.AppointmentDate)
            .MinAsync();

        decimal averageAppointmentsPerWeek = 0m;

        if (firstAppointmentDate.HasValue && totalAppointments > 0)
        {
            var days = (today - firstAppointmentDate.Value.Date).TotalDays;

            var weeks = Math.Max(days / 7, 1);

            averageAppointmentsPerWeek =
                totalAppointments / (decimal)weeks;
        }

        var patientIds = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                AppointmentCount = g.Count()
            })
            .ToListAsync();

        var newPatients = patientIds.Count(x => x.AppointmentCount == 1);

        var returningPatients = patientIds.Count(x => x.AppointmentCount > 1);

        return new DoctorStatsSummaryViewModel
        {
            TotalAppointments = totalAppointments,

            CompletedAppointments = completedAppointments,

            CancelledAppointments = cancelledAppointments,

            UpcomingAppointments = upcomingAppointments,

            TotalPatients = totalPatients,

            NewPatients = newPatients,

            ReturningPatients = returningPatients,

            TotalEarnings = totalEarnings,

            OnlineEarnings = onlineEarnings,

            OnsiteEarnings = onsiteEarnings,

            AverageConsultationFee = averageConsultationFee,

            AverageRevenuePerAppointment = averageRevenuePerAppointment,

            CompletionRate = completionRate,

            CancellationRate = cancellationRate,

            AverageAppointmentsPerWeek = averageAppointmentsPerWeek,

            OnlineAppointments = onlineAppointments,

            OnsiteAppointments = onsiteAppointments,

        };
    }


    // =========================================================
    // FINANCIAL ANALYTICS
    // =========================================================

    private async Task<DoctorStatsFinancialViewModel> GetFinancialAnalyticsAsync(
        int doctorId)
    {
        var revenueOverTime = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Timestamp != null &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .GroupBy(t => t.Timestamp!.Value.Date)
            .Select(g => new DoctorStatsRevenueDataPointViewModel
            {
                Date = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();


        var monthlyRevenueData = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Timestamp != null &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .GroupBy(t => new
            {
                Year = t.Timestamp!.Value.Year,
                Month = t.Timestamp!.Value.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        var monthlyRevenue = monthlyRevenueData
            .Select(x => new DoctorStatsMonthlyRevenueViewModel
            {
                Year = x.Year,
                Month = x.Month,
                MonthName = new DateTime(x.Year, x.Month, 1)
                    .ToString("MMM yyyy"),
                Amount = x.Amount
            })
            .ToList();


        var highestRevenueMonth = monthlyRevenue
            .OrderByDescending(x => x.Amount)
            .FirstOrDefault();


        var revenueByPaymentMethod = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new DoctorStatsPaymentMethodViewModel
            {
                PaymentMethod = g.Key ?? "Unknown",
                Amount = g.Sum(t => t.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();


        var paymentStatus = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .GroupBy(t => t.Status)
            .Select(g => new DoctorStatsPaymentStatusViewModel
            {
                Status = g.Key ?? "Unknown",
                Count = g.Count(),
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        return new DoctorStatsFinancialViewModel
        {
            RevenueOverTime = revenueOverTime,

            MonthlyRevenue = monthlyRevenue,

            RevenueByPaymentMethod = revenueByPaymentMethod,

            PaymentStatus = paymentStatus,

            HighestRevenueMonth =
                highestRevenueMonth?.MonthName ?? "No data",

            HighestMonthlyRevenue =
                highestRevenueMonth?.Amount ?? 0m
        };
    }


    // =========================================================
    // APPOINTMENT ANALYTICS
    // =========================================================

    private async Task<DoctorStatsAppointmentViewModel> GetAppointmentAnalyticsAsync(
        int doctorId)
    {
        var appointmentsOverTime = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => a.AppointmentDate.Date)
            .Select(g => new DoctorStatsAppointmentTrendViewModel
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();


        var monthlyAppointmentData = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => new
            {
                Year = a.AppointmentDate.Year,
                Month = a.AppointmentDate.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        var monthlyAppointments = monthlyAppointmentData
            .Select(x => new DoctorStatsMonthlyAppointmentViewModel
            {
                Year = x.Year,
                Month = x.Month,
                MonthName = new DateTime(x.Year, x.Month, 1)
                    .ToString("MMM yyyy"),
                Count = x.Count
            })
            .ToList();


        var weekdayData = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.AppointmentDate.DayOfWeek)
            .ToListAsync();

        var weekdayOrder = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        var appointmentsByWeekday = weekdayData
            .GroupBy(day => day)
            .Select(g => new DoctorStatsWeekdayViewModel
            {
                Day = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(x =>
                Array.IndexOf(
                    weekdayOrder,
                    Enum.Parse<DayOfWeek>(x.Day)))
            .ToList();


        var appointmentStatus = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => a.Status ?? "unknown")
            .Select(g => new DoctorStatsStatusViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        /*
         * We use Appointment.PaymentMethod here because this is
         * appointment-level information.
         *
         * Expected values in your current system:
         * cash
         * online
         */

        var appointmentsByMethod = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => a.PaymentMethod ?? "unknown")
            .Select(g => new DoctorStatsAppointmentMethodViewModel
            {
                Method = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        var busiestMonth = monthlyAppointments
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        var busiestWeekday = appointmentsByWeekday
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();


        return new DoctorStatsAppointmentViewModel
        {
            AppointmentsOverTime = appointmentsOverTime,

            MonthlyAppointments = monthlyAppointments,

            AppointmentsByWeekday = appointmentsByWeekday,

            AppointmentStatus = appointmentStatus,

            AppointmentsByMethod = appointmentsByMethod,

            BusiestMonth =
                busiestMonth?.MonthName ?? "No data",

            BusiestMonthAppointmentCount =
                busiestMonth?.Count ?? 0,

            BusiestWeekday =
                busiestWeekday?.Day ?? "No data",

            BusiestWeekdayAppointmentCount =
                busiestWeekday?.Count ?? 0
        };
    }


    // =========================================================
    // PATIENT ANALYTICS
    // =========================================================

    private async Task<DoctorStatsPatientViewModel> GetPatientAnalyticsAsync(
        int doctorId)
    {
        var mostVisitedPatients = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => new
            {
                a.PatientId,
                PatientName = a.Patient!.FullName
            })
            .Select(g => new DoctorStatsTopPatientViewModel
            {
                PatientId = g.Key.PatientId,

                PatientName = g.Key.PatientName ?? "Unknown",

                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.AppointmentCount)
            .Take(10)
            .ToListAsync();


        var highestSpendingPatients = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Appointment != null &&
                t.Appointment.DoctorId == doctorId)
            .GroupBy(t => new
            {
                t.PatientId,
                PatientName = t.Patient!.FullName
            })
            .Select(g => new DoctorStatsTopSpendingPatientViewModel
            {
                PatientId = g.Key.PatientId,

                PatientName = g.Key.PatientName ?? "Unknown",

                TotalSpent = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(10)
            .ToListAsync();


        var patientAppointmentCounts = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                AppointmentCount = g.Count()
            })
            .ToListAsync();


        var patientTypes = new List<DoctorStatsPatientTypeViewModel>
        {
            new()
            {
                Type = "New Patients",
                Count = patientAppointmentCounts.Count(
                    x => x.AppointmentCount == 1)
            },

            new()
            {
                Type = "Returning Patients",
                Count = patientAppointmentCounts.Count(
                    x => x.AppointmentCount > 1)
            }
        };


        return new DoctorStatsPatientViewModel
        {
            MostVisitedPatients = mostVisitedPatients,

            HighestSpendingPatients = highestSpendingPatients,

            PatientTypes = patientTypes
        };
    }


    // =========================================================
    // UPCOMING APPOINTMENTS
    // =========================================================

    private async Task<DoctorStatsUpcomingViewModel> GetUpcomingAnalyticsAsync(
        int doctorId)
    {
        var today = DateTime.Today;

        var tomorrow = today.AddDays(1);

        var next7Days = today.AddDays(7);

        var next30Days = today.AddDays(30);


        var baseQuery = _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate >= today &&
                a.Status != "cancelled");


        var todayCount = await baseQuery
            .CountAsync(a =>
                a.AppointmentDate >= today &&
                a.AppointmentDate < tomorrow);


        var tomorrowCount = await baseQuery
            .CountAsync(a =>
                a.AppointmentDate >= tomorrow &&
                a.AppointmentDate < tomorrow.AddDays(1));


        var next7DaysCount = await baseQuery
            .CountAsync(a =>
                a.AppointmentDate >= today &&
                a.AppointmentDate < next7Days);


        var next30DaysCount = await baseQuery
            .CountAsync(a =>
                a.AppointmentDate >= today &&
                a.AppointmentDate < next30Days);


        var upcomingAppointments = await baseQuery
            .OrderBy(a => a.AppointmentDate)
            .Take(20)
            .Select(a => new DoctorStatsUpcomingAppointmentViewModel
            {
                AppointmentId = a.AppointmentId,

                PatientId = a.PatientId,

                PatientName = a.Patient!.FullName ?? "Unknown",

                AppointmentDate = a.AppointmentDate,

                Status = a.Status ?? "Unknown",

                PaymentStatus = a.PaymentStatus ?? "Unknown",

                PaymentMethod = a.PaymentMethod ?? "Unknown",

                ConsultationFee = a.ConsultationFee
            })
            .ToListAsync();


        return new DoctorStatsUpcomingViewModel
        {
            Today = todayCount,

            Tomorrow = tomorrowCount,

            Next7Days = next7DaysCount,

            Next30Days = next30DaysCount,

            Appointments = upcomingAppointments
        };
    }
}
