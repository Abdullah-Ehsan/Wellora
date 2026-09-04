using System;

namespace Wellora.Areas.Admin.ViewModels.AdminDashboard
{
    public class AdminDashboardViewModel
    {
        // Basic IDs
        public int AdminId { get; set; }
        public int UserId { get; set; }

        // User info
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";

        // Admin info
        public string? ProfilePicture { get; set; }
        public string? Gender { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;

                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;

                if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;

                return age;
            }
        }

        public string? AdminType { get; set; }
        public string? Status { get; set; }
    }
}