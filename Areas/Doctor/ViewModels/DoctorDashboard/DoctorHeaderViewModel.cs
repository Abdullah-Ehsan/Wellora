namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class DoctorHeaderViewModel
    {
        public int DoctorId { get; set; } 

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? ProfilePhoto { get; set; }

        public string? Specialization { get; set; }
    }
}