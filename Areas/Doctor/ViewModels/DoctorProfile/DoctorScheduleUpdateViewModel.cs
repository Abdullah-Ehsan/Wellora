namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class DoctorScheduleUpdateViewModel
    {
        public int DoctorId { get; set; }

        public List<DoctorScheduleRow> ScheduleRows { get; set; } = new();

        public List<DoctorBreakViewModel> Breaks { get; set; } = new();
    }
}
