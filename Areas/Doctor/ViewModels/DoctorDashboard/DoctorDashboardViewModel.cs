namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public DoctorHeaderViewModel Header { get; set; } = new();

        public DoctorPersonalInfoViewModel PersonalInfo { get; set; } = new();

        public List<TodayAppointmentViewModel> TodayAppointments { get; set; } = new();

        public PatientStatsViewModel PatientStats { get; set; } = new();

        public RevenueViewModel Revenue { get; set; } = new();

        public GraphDataViewModel Graphs { get; set; } = new();

        public ClinicalPracticeViewModel ClinicalPractice { get; set; } = new();

        public List<WeeklyScheduleRowViewModel> WeeklySchedule { get; set; } = new();

        public CredentialsViewModel Credentials { get; set; } = new();

        public SpecialtiesViewModel Specialties { get; set; } = new();

        public PublicationViewModel Publications { get; set; } = new();
    }
}