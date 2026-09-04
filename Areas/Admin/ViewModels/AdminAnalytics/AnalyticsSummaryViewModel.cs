namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class AnalyticsSummaryViewModel
{
    public decimal TotalRevenue { get; set; }

    public int TotalAppointments { get; set; }

    public int TotalPatients { get; set; }

    public int TotalDoctors { get; set; }

    public decimal AverageConsultationFee { get; set; }
}
