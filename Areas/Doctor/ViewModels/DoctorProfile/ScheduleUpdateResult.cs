namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class ScheduleUpdateResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateOnly? EffectiveFrom { get; set; }

        public int ChangesThisMonth { get; set; }

        public int RemainingChanges { get; set; }
    }

}
