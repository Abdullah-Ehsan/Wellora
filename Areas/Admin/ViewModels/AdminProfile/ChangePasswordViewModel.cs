using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Admin.ViewModels.AdminProfile
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "The new passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
