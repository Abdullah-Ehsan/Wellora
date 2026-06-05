namespace Wellora.Areas.Patient.ViewModels.PatientProfile
{
    public class MedicalHistoryViewModel
    {
        public string? Allergies { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Medications { get; set; }
        public string? BloodGroup { get; set; } // e.g. "A+" or "O-"
    }
}
