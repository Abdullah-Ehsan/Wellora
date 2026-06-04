namespace Wellora.Areas.Patient.ViewModels.DoctorInformation
{
    public class DoctorScheduleViewModel
    {
        public string? DayOfWeek { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int AppointmentDurationMin { get; set; }
        public int MaxPatientsPerDay { get; set; }
        public string? BreakStart { get; set; }
        public string? BreakEnd { get; set; }
    }

}
