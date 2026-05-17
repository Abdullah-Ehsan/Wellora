using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Admin.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        public string LoginIdentifier { get; set; } // can be email or username

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}