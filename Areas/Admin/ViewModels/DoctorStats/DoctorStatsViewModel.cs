namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string? ProfilePhoto { get; set; }

    public int? YearsExperience { get; set; }

    public string Country { get; set; } = string.Empty;

    public decimal ConsultationFee { get; set; }

    public DoctorStatsSummaryViewModel Summary { get; set; } = new();

    public DoctorStatsFinancialViewModel Financial { get; set; } = new();

    public DoctorStatsAppointmentViewModel Appointments { get; set; } = new();

    public DoctorStatsPatientViewModel Patients { get; set; } = new();

    public DoctorStatsUpcomingViewModel Upcoming { get; set; } = new();
}
