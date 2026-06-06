namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class TodayAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? PatientPhoto { get; set; }

        public string? Allergies { get; set; }

        public DateTime AppointmentDate { get; set; }
    }
}