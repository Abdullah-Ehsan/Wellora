using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Patient.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string LoginIdentifier { get; set; } // can be email or username

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

}
