namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class PatientSummaryViewModel
    {
        public int PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? PatientPhoto { get; set; }

        public int AppointmentCount { get; set; }

        public decimal TotalSpent { get; set; }

        public DateTime? LastAppointmentDate { get; set; }
    }
}