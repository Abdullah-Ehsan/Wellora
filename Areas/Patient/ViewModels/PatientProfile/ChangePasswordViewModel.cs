using System.ComponentModel.DataAnnotations;

namespace Wellora.Areas.Patient.ViewModels.PatientProfile
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your current password.")]
        public string OldPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please enter a new password.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
