using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Admin.Services.AdminAnalytics.Interfaces;
using Wellora.Areas.Admin.ViewModels.AdminAnalytics;
using Wellora.Data;

namespace Wellora.Areas.Admin.Services.AdminAnalytics.Services;

public class AdminAnalyticsService : IAdminAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AdminAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAnalyticsViewModel> GetAnalyticsAsync()
    {
        return new AdminAnalyticsViewModel
        {
            Summary = await GetSummaryAsync(),
            Financial = await GetFinancialAnalyticsAsync(),
            Appointments = await GetAppointmentAnalyticsAsync(),
            Patients = await GetPatientAnalyticsAsync(),
            Doctors = await GetDoctorAnalyticsAsync()
        };
    }


    // =========================================================
    // SUMMARY
    // =========================================================

    private async Task<AnalyticsSummaryViewModel> GetSummaryAsync()
    {
        var totalRevenue = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Status == "paid")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var totalAppointments = await _context.Appointments
            .AsNoTracking()
            .CountAsync();

        var totalPatients = await _context.Patients
            .AsNoTracking()
            .CountAsync();

        var totalDoctors = await _context.Doctors
            .AsNoTracking()
            .CountAsync();

        var averageConsultationFee = await _context.Appointments
            .AsNoTracking()
            .AverageAsync(a => (decimal?)a.ConsultationFee) ?? 0m;

        return new AnalyticsSummaryViewModel
        {
            TotalRevenue = totalRevenue,
            TotalAppointments = totalAppointments,
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            AverageConsultationFee = averageConsultationFee
        };
    }


    // =========================================================
    // FINANCIAL ANALYTICS
    // =========================================================

    private async Task<FinancialAnalyticsViewModel> GetFinancialAnalyticsAsync()
    {
        var revenueOverTime = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Timestamp != null)
            .GroupBy(t => t.Timestamp!.Value.Date)
            .Select(g => new RevenueDataPointViewModel
            {
                Date = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();


        var paymentStatus = await _context.Transactions
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // Only cash and online are used.
        var paymentMethods = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.PaymentMethod == "cash" ||
                t.PaymentMethod == "online")
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new PaymentMethodCountViewModel
            {
                PaymentMethod = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        var revenueByPaymentMethod = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                (t.PaymentMethod == "cash" ||
                 t.PaymentMethod == "online"))
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new PaymentMethodRevenueViewModel
            {
                PaymentMethod = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();


        var transactionOutcomes = await _context.Transactions
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        return new FinancialAnalyticsViewModel
        {
            RevenueOverTime = revenueOverTime,
            PaymentStatus = paymentStatus,
            PaymentMethods = paymentMethods,
            RevenueByPaymentMethod = revenueByPaymentMethod,
            TransactionOutcomes = transactionOutcomes
        };
    }


    // =========================================================
    // APPOINTMENT ANALYTICS
    // =========================================================

    private async Task<AppointmentAnalyticsViewModel> GetAppointmentAnalyticsAsync()
    {
        var appointmentsOverTime = await _context.Appointments
            .AsNoTracking()
            .GroupBy(a => a.AppointmentDate.Date)
            .Select(g => new AppointmentTrendViewModel
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();


        var appointmentStatus = await _context.Appointments
            .AsNoTracking()
            .GroupBy(a => a.Status ?? "unknown")
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        var appointmentPaymentStatus = await _context.Appointments
            .AsNoTracking()
            .GroupBy(a => a.PaymentStatus ?? "unknown")
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // Load only the weekday value, then group in memory.
        // This avoids provider-specific DayOfWeek translation issues.
        var appointmentDates = await _context.Appointments
            .AsNoTracking()
            .Select(a => a.AppointmentDate)
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

        var appointmentsByWeekday = appointmentDates
            .GroupBy(date => date.DayOfWeek)
            .Select(g => new WeekdayCountViewModel
            {
                Day = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(x =>
                Array.IndexOf(
                    weekdayOrder,
                    Enum.Parse<DayOfWeek>(x.Day)))
            .ToList();


        var consultationRevenue = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.Status == "scheduled" ||
                a.Status == "checked")
            .GroupBy(a => a.AppointmentDate.Date)
            .Select(g => new RevenueDataPointViewModel
            {
                Date = g.Key,
                Amount = g.Sum(a => a.ConsultationFee)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();


        return new AppointmentAnalyticsViewModel
        {
            AppointmentsOverTime = appointmentsOverTime,
            AppointmentStatus = appointmentStatus,
            AppointmentPaymentStatus = appointmentPaymentStatus,
            AppointmentsByWeekday = appointmentsByWeekday,
            ConsultationRevenue = consultationRevenue
        };
    }


    // =========================================================
    // PATIENT ANALYTICS
    // =========================================================

    private async Task<PatientAnalyticsViewModel> GetPatientAnalyticsAsync()
    {
        // A "visit" means a scheduled or checked appointment.
        // Cancelled appointments are excluded.
        var topVisitedPatients = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.Status == "scheduled" ||
                a.Status == "checked")
            .GroupBy(a => a.PatientId)
            .Select(g => new PatientVisitViewModel
            {
                PatientId = g.Key,
                PatientName = _context.Patients
                    .Where(p => p.PatientId == g.Key)
                    .Select(p => p.FullName)
                    .FirstOrDefault() ?? "Unknown",
                VisitCount = g.Count()
            })
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.PatientName)
            .Take(10)
            .ToListAsync();


        // Spending only comes from successful payments.
        var topSpendingPatients = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Status == "paid")
            .GroupBy(t => t.PatientId)
            .Select(g => new PatientSpendingViewModel
            {
                PatientId = g.Key,
                PatientName = _context.Patients
                    .Where(p => p.PatientId == g.Key)
                    .Select(p => p.FullName)
                    .FirstOrDefault() ?? "Unknown",
                TotalSpent = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .ThenBy(x => x.PatientName)
            .Take(10)
            .ToListAsync();


        var genderDistribution = await _context.Patients
            .AsNoTracking()
            .GroupBy(p => p.Gender ?? "unknown")
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // DateOnly is converted to age in memory.
        var patientBirthDates = await _context.Patients
            .AsNoTracking()
            .Select(p => p.DateOfBirth)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var ageGroups = patientBirthDates
            .Select(dateOfBirth =>
            {
                var age = today.Year - dateOfBirth.Year;

                if (dateOfBirth > today.AddYears(-age))
                {
                    age--;
                }

                return age;
            })
            .GroupBy(GetPatientAgeGroup)
            .Select(g => new AgeGroupViewModel
            {
                AgeGroup = g.Key,
                Count = g.Count()
            })
            .ToList();

        var ageGroupOrder = new[]
        {
            "Under 18",
            "18-25",
            "26-35",
            "36-45",
            "46-55",
            "56-65",
            "66+"
        };

        ageGroups = ageGroups
            .OrderBy(x => Array.IndexOf(ageGroupOrder, x.AgeGroup))
            .ToList();


        var preferredLanguages = await _context.Patients
            .AsNoTracking()
            .Where(p => !string.IsNullOrWhiteSpace(p.PreferredLanguage))
            .GroupBy(p => p.PreferredLanguage!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Category)
            .ToListAsync();


        var patientsByPrimaryDoctor = await _context.Patients
            .AsNoTracking()
            .Where(p => p.PrimaryDoctorId != null)
            .GroupBy(p => p.PrimaryDoctorId!.Value)
            .Select(g => new DoctorPatientCountViewModel
            {
                DoctorId = g.Key,
                DoctorName = _context.Doctors
                    .Where(d => d.DoctorId == g.Key)
                    .Select(d => d.FullName)
                    .FirstOrDefault() ?? "Unknown",
                PatientCount = g.Count()
            })
            .OrderByDescending(x => x.PatientCount)
            .ThenBy(x => x.DoctorName)
            .ToListAsync();


        return new PatientAnalyticsViewModel
        {
            TopVisitedPatients = topVisitedPatients,
            TopSpendingPatients = topSpendingPatients,
            GenderDistribution = genderDistribution,
            AgeGroups = ageGroups,
            PreferredLanguages = preferredLanguages,
            PatientsByPrimaryDoctor = patientsByPrimaryDoctor
        };
    }


    // =========================================================
    // DOCTOR ANALYTICS
    // =========================================================

    private async Task<DoctorAnalyticsViewModel> GetDoctorAnalyticsAsync()
    {
        // =========================================================
        // SPECIALIZATIONS
        // =========================================================

        var specializations = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
            .GroupBy(d => d.Specialization!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // SUB-SPECIALTIES
        // =========================================================

        var doctorsWithSubSpecialties = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.SubSpecialties))
            .Select(d => d.SubSpecialties!)
            .ToListAsync();

        var subSpecialties = doctorsWithSubSpecialties
            .SelectMany(value => value
                .Split(
                    new[] { ',', ';' },
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries))
            .GroupBy(value => value)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();


        // =========================================================
        // PRIMARY MEDICAL DEGREES
        // =========================================================

        var primaryMedicalDegrees = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.PrimaryMedicalDegree))
            .GroupBy(d => d.PrimaryMedicalDegree!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // POSTGRADUATE DEGREES
        // =========================================================

        var postgraduateDegrees = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.PostgraduateDegree))
            .GroupBy(d => d.PostgraduateDegree!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // SUPER SPECIALTIES
        // =========================================================

        var superSpecialties = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.SuperSpecialty))
            .GroupBy(d => d.SuperSpecialty!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // PROFESSIONAL CERTIFICATIONS
        // =========================================================

        var professionalCertifications = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.ProfessionalCertification))
            .GroupBy(d => d.ProfessionalCertification!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // MEDICAL SCHOOLS
        // =========================================================

        var medicalSchools = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.MedicalSchool))
            .GroupBy(d => d.MedicalSchool!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // GENDER DISTRIBUTION
        // =========================================================

        var genderDistribution = await _context.Doctors
            .AsNoTracking()
            .GroupBy(d => d.Gender ?? "unknown")
            .Select(g => new StatusCountViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // EXPERIENCE GROUPS
        // =========================================================

        var experienceValues = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.YearsExperience != null)
            .Select(d => d.YearsExperience!.Value)
            .ToListAsync();

        var experienceGroups = experienceValues
            .GroupBy(years =>
            {
                if (years <= 5)
                    return "0-5 years";

                if (years <= 10)
                    return "6-10 years";

                if (years <= 20)
                    return "11-20 years";

                if (years <= 30)
                    return "21-30 years";

                return "31+ years";
            })
            .Select(g => new ExperienceGroupViewModel
            {
                Range = g.Key,
                Count = g.Count()
            })
            .ToList();

        var experienceOrder = new[]
        {
        "0-5 years",
        "6-10 years",
        "11-20 years",
        "21-30 years",
        "31+ years"
    };

        experienceGroups = experienceGroups
            .OrderBy(x => Array.IndexOf(experienceOrder, x.Range))
            .ToList();


        // =========================================================
        // CONSULTATION FEE DISTRIBUTION
        // =========================================================

        var feeValues = await _context.Doctors
            .AsNoTracking()
            .Select(d => d.ConsultationFee)
            .ToListAsync();

        var feeDistribution = feeValues
            .GroupBy(fee =>
            {
                if (fee < 1000)
                    return "Under 1,000";

                if (fee < 2500)
                    return "1,000 - 2,499";

                if (fee < 5000)
                    return "2,500 - 4,999";

                if (fee < 10000)
                    return "5,000 - 9,999";

                return "10,000+";
            })
            .Select(g => new FeeRangeViewModel
            {
                Range = g.Key,
                Count = g.Count()
            })
            .ToList();

        var feeOrder = new[]
        {
        "Under 1,000",
        "1,000 - 2,499",
        "2,500 - 4,999",
        "5,000 - 9,999",
        "10,000+"
    };

        feeDistribution = feeDistribution
            .OrderBy(x => Array.IndexOf(feeOrder, x.Range))
            .ToList();


        // =========================================================
        // TELEMEDICINE AVAILABILITY
        // =========================================================

        var telemedicineAvailability = await _context.Doctors
            .AsNoTracking()
            .GroupBy(d => d.TelemedicineAvailable == true
                ? "Available"
                : "Not Available")
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .ToListAsync();


        // =========================================================
        // COUNTRIES
        // =========================================================

        var countries = await _context.Doctors
            .AsNoTracking()
            .Where(d => !string.IsNullOrWhiteSpace(d.Country))
            .GroupBy(d => d.Country!)
            .Select(g => new CategoryCountViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // =========================================================
        // BUSIEST DOCTORS
        // =========================================================

        var busiestDoctors = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Doctor != null)
            .GroupBy(a => new
            {
                a.DoctorId,
                DoctorName = a.Doctor!.FullName,
                Specialization = a.Doctor.Specialization
            })
            .Select(g => new DoctorWorkloadViewModel
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName ?? "Unknown",
                Specialization = g.Key.Specialization ?? "Unknown",
                AppointmentCount = g.Count()
            })
            .OrderByDescending(x => x.AppointmentCount)
            .Take(10)
            .ToListAsync();


        // =========================================================
        // DOCTOR REVENUE
        // =========================================================

        var doctorRevenue = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == "paid" &&
                t.Appointment != null &&
                t.Appointment.Doctor != null)
            .GroupBy(t => new
            {
                DoctorId = t.Appointment!.DoctorId,
                DoctorName = t.Appointment.Doctor!.FullName
            })
            .Select(g => new DoctorRevenueViewModel
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName ?? "Unknown",
                Revenue = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();


        // =========================================================
        // DOCTOR PERFORMANCE
        // =========================================================

        var performance = await _context.Doctors
            .AsNoTracking()
            .Select(d => new DoctorPerformanceViewModel
            {
                DoctorId = d.DoctorId,
                DoctorName = d.FullName ?? "Unknown",
                Specialization = d.Specialization ?? "Unknown",

                TotalAppointments = _context.Appointments
                    .Count(a => a.DoctorId == d.DoctorId),

                CompletedAppointments = _context.Appointments
                    .Count(a =>
                        a.DoctorId == d.DoctorId &&
                        a.Status == "checked"),

                CancelledAppointments = _context.Appointments
                    .Count(a =>
                        a.DoctorId == d.DoctorId &&
                        a.Status == "cancelled"),

                Revenue = _context.Transactions
                    .Where(t =>
                        t.Status == "paid" &&
                        t.Appointment != null &&
                        t.Appointment.DoctorId == d.DoctorId)
                    .Sum(t => (decimal?)t.Amount) ?? 0m,

                AverageFee = _context.Appointments
                    .Where(a => a.DoctorId == d.DoctorId)
                    .Average(a => (decimal?)a.ConsultationFee) ?? 0m
            })
            .OrderByDescending(x => x.TotalAppointments)
            .ToListAsync();


        // =========================================================
        // RETURN
        // =========================================================

        return new DoctorAnalyticsViewModel
        {
            Specializations = specializations,
            SubSpecialties = subSpecialties,

            PrimaryMedicalDegrees = primaryMedicalDegrees,
            PostgraduateDegrees = postgraduateDegrees,
            SuperSpecialties = superSpecialties,
            ProfessionalCertifications = professionalCertifications,
            MedicalSchools = medicalSchools,

            GenderDistribution = genderDistribution,
            ExperienceGroups = experienceGroups,
            ConsultationFeeDistribution = feeDistribution,
            TelemedicineAvailability = telemedicineAvailability,
            Countries = countries,
            BusiestDoctors = busiestDoctors,
            DoctorRevenue = doctorRevenue,
            Performance = performance
        };
    }



    // =========================================================
    // HELPERS
    // =========================================================

    private static string GetPatientAgeGroup(int age)
    {
        if (age < 18)
            return "Under 18";

        if (age <= 25)
            return "18-25";

        if (age <= 35)
            return "26-35";

        if (age <= 45)
            return "36-45";

        if (age <= 55)
            return "46-55";

        if (age <= 65)
            return "56-65";

        return "66+";
    }


    private static string GetExperienceGroup(int years)
    {
        if (years <= 5)
            return "0-5 years";

        if (years <= 10)
            return "6-10 years";

        if (years <= 20)
            return "11-20 years";

        if (years <= 30)
            return "21-30 years";

        return "31+ years";
    }


    private static string GetFeeRange(decimal fee)
    {
        if (fee < 1000)
            return "Under 1,000";

        if (fee < 2500)
            return "1,000 - 2,499";

        if (fee < 5000)
            return "2,500 - 4,999";

        if (fee < 10000)
            return "5,000 - 9,999";

        return "10,000+";
    }
}
