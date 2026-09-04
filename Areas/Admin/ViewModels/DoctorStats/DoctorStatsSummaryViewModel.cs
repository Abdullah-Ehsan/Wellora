namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsSummaryViewModel
{
    public int TotalAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }

    public int UpcomingAppointments { get; set; }

    public int TotalPatients { get; set; }

    public int NewPatients { get; set; }

    public int ReturningPatients { get; set; }

    public decimal TotalEarnings { get; set; }

    public decimal OnlineEarnings { get; set; }

    public decimal OnsiteEarnings { get; set; }

    public decimal AverageConsultationFee { get; set; }

    public decimal AverageRevenuePerAppointment { get; set; }

    public decimal CompletionRate { get; set; }

    public decimal CancellationRate { get; set; }

    public decimal AverageAppointmentsPerWeek { get; set; }

    public int OnlineAppointments { get; set; }
    public int OnsiteAppointments { get; set; }

}
