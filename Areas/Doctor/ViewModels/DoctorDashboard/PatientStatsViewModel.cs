namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class PatientStatsViewModel
    {
        public PatientSummaryViewModel HighestSpendingPatient { get; set; } = new();

        public PatientSummaryViewModel MostVisitedPatient { get; set; } = new();
    }
}