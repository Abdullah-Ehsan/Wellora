namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class AppointmentAnalyticsViewModel
{
    public List<AppointmentTrendViewModel> AppointmentsOverTime { get; set; } = [];

    public List<StatusCountViewModel> AppointmentStatus { get; set; } = [];

    public List<StatusCountViewModel> AppointmentPaymentStatus { get; set; } = [];

    public List<WeekdayCountViewModel> AppointmentsByWeekday { get; set; } = [];

    public List<RevenueDataPointViewModel> ConsultationRevenue { get; set; } = [];
}


public class AppointmentTrendViewModel
{
    public DateTime Date { get; set; }

    public int Count { get; set; }
}


public class WeekdayCountViewModel
{
    public string Day { get; set; } = string.Empty;

    public int Count { get; set; }
}
