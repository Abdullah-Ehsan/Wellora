namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }

        public List<DoctorScheduleRow> ScheduleRows { get; set; } = new();

        public List<DoctorBreakViewModel> Breaks { get; set; } = new();
    }

    
        public class DoctorScheduleRow
        {
            public int ScheduleId { get; set; }

            public int DayOfWeek { get; set; }

            public TimeSpan? StartTime { get; set; }

            public TimeSpan? EndTime { get; set; }

            public int? MaxPatientsPerDay { get; set; }

            public int? AppointmentDurationMin { get; set; }

            public int? BufferTimeMin { get; set; }

            // UI only:
            // true  = On
            // false = Off
            // null  = -
            public bool? Status { get; set; }
        }
    


}
