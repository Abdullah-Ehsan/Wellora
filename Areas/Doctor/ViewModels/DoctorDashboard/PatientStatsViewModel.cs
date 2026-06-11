namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class PatientStatsViewModel
    {
        public List<PatientSummaryViewModel> TopSpendingPatients { get; set; } = new();

        public List<PatientSummaryViewModel> TopVisitedPatients { get; set; } = new();
    }
}