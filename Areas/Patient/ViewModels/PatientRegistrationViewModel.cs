using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Patient.ViewModels
{
    public class PatientRegistrationViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters, include one uppercase letter, one number, " +
            "and one special character.")]
        public string Password { get; set; }


        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}
