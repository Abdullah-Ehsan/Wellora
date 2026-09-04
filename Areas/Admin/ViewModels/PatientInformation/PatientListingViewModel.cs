
namespace Wellora.Areas.Admin.ViewModels.PatientInformation
{
    using System.Collections.Generic;
    using Wellora.Areas.Patient.Models;
    public class PatientListingViewModel
    {
        public IEnumerable<Patient>? Patients { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Filters
        public string? SelectedGender { get; set; }
        public string? SelectedLanguage { get; set; }
    }
}
