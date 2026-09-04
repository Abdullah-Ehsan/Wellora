namespace Wellora.ViewModels.PatientDetail
{
    public class PrimaryDoctorViewModel
    {
        public bool Exists { get; set; }

        public bool IsOutsideDoctor { get; set; }

        public string? DoctorName { get; set; }

        public string? Specialty { get; set; }

        public string? ContactNumber { get; set; }

        public string? ProfilePhoto { get; set; }

        public string? HospitalName { get; set; }

        public string? HospitalAddress { get; set; }
    }
}
