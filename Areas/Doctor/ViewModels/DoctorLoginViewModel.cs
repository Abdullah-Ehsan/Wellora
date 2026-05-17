using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Doctor.ViewModels
{
    public class DoctorLoginViewModel
    {
        [Required]
        public string LoginIdentifier { get; set; } // can be email or username

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

}
