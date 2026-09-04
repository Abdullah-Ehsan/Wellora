namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsUpcomingViewModel
{
    public int Today { get; set; }

    public int Tomorrow { get; set; }

    public int Next7Days { get; set; }

    public int Next30Days { get; set; }

    public List<DoctorStatsUpcomingAppointmentViewModel> Appointments { get; set; } = [];
}


public class DoctorStatsUpcomingAppointmentViewModel
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal ConsultationFee { get; set; }
}
