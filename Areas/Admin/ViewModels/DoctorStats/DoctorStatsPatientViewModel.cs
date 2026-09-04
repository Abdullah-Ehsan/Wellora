namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsPatientViewModel
{
    public List<DoctorStatsTopPatientViewModel> MostVisitedPatients { get; set; } = [];

    public List<DoctorStatsTopSpendingPatientViewModel> HighestSpendingPatients { get; set; } = [];

    public List<DoctorStatsPatientTypeViewModel> PatientTypes { get; set; } = [];
}


public class DoctorStatsTopPatientViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public int AppointmentCount { get; set; }
}


public class DoctorStatsTopSpendingPatientViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public decimal TotalSpent { get; set; }
}


public class DoctorStatsPatientTypeViewModel
{
    public string Type { get; set; } = string.Empty;

    public int Count { get; set; }
}
