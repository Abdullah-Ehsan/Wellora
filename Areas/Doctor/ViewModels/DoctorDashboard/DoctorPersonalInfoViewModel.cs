namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class DoctorPersonalInfoViewModel
    {
        public DateOnly DateOfBirth { get; set; }

        public int Age
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - DateOfBirth.Year;

                if (today < DateOfBirth.AddYears(age))
                    age--;

                return age;
            }
        }

        public string? Gender { get; set; }

        public string DisplayGender =>
            Gender?.ToLower() switch
            {
                "male" => "Male",
                "female" => "Female",
                "other" => "Other",
                _ => "Not Specified"
            };

        public string? Country { get; set; }

        public string? ContactNumber { get; set; }

        public string? HospitalAddress { get; set; }

        public string? LanguagesSpoken { get; set; }
    }
}