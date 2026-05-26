namespace Wellora.Areas.Patient.ViewModels
{
    using Wellora.Areas.Doctor.Models;
    using System.Collections.Generic;
    public class DoctorListingViewModel
    {
        public IEnumerable<Doctor> Doctors { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Filters
        public string SelectedSpecialty { get; set; }
        public string SelectedLanguage { get; set; }
        public string SelectedGender { get; set; }
    }
}
