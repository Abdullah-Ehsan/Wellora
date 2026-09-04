using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Doctor.ViewModels.DoctorProfile
{
    public class ChangePasswordViewModel
    {
        public int UserId { get; set; }

        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }


}
