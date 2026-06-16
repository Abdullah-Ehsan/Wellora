namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }
        public List<DoctorScheduleRow> ScheduleRows { get; set; } = new();
    }

    public class DoctorScheduleRow
    {
        public int DayOfWeek { get; set; } // 1=Mon ... 7=Sun
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
        public int MaxPatientsPerDay { get; set; }
        public int AppointmentDurationMin { get; set; }
        public bool IsActive { get; set; }
    }

}
