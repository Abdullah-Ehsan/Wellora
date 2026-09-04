using Wellora.Areas.Admin.ViewModels.AdminAnalytics;

namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class AdminAnalyticsViewModel
{
    public AnalyticsSummaryViewModel Summary { get; set; } = new();

    public FinancialAnalyticsViewModel Financial { get; set; } = new();

    public AppointmentAnalyticsViewModel Appointments { get; set; } = new();

    public PatientAnalyticsViewModel Patients { get; set; } = new();

    public DoctorAnalyticsViewModel Doctors { get; set; } = new();
}
