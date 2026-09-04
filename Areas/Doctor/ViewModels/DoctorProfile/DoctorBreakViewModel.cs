namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorBreakViewModel
    {
        public int BreakId { get; set; }
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
    }

}
