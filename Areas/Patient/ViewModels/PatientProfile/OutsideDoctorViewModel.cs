namespace Wellora.Areas.Patient.ViewModels.PatientProfile
{
    public class OutsideDoctorViewModel
    {
        public string DoctorName { get; set; } = string.Empty;

        public string? DoctorSpecialty { get; set; }

        public string? HospitalName { get; set; }

        public string? HospitalCity { get; set; }

        public string? HospitalCountry { get; set; }

        public string? DoctorPhone { get; set; }

        public string? DoctorEmail { get; set; }

        public string? DoctorPhoto { get; set; }
    }
}
