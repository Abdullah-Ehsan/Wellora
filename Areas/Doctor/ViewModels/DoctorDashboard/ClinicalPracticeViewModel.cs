namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class ClinicalPracticeViewModel
    {
        public int? YearsExperience { get; set; }

        public bool? TelemedicineAvailable { get; set; }

        public decimal ConsultationFee { get; set; }

        public bool? DoctorAvailable { get; set; }
    }
}