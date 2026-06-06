namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class WeeklyScheduleRowViewModel
    {
        public string DayName { get; set; } = string.Empty;

        public string WorkingHours { get; set; } = "-";

        public string BreakTime { get; set; } = "-";

        public string Duration { get; set; } = "-";

        public string MaxPatients { get; set; } = "-";

        public bool IsOffDay { get; set; }
    }
}