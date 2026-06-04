namespace Wellora.Areas.Patient.ViewModels.MakeAppointment
{
    public class SlotItem
    {
        public string? Time { get; set; }        // "09:35 AM"
        public bool IsAvailable { get; set; }   // clickable or not
    }
}