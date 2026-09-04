using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Wellora.Areas.Admin.ViewModels.AdminProfile
{
    public class ProfileInfoViewModel
    {
        // =========================================================
        // PROFILE PICTURE
        // =========================================================

        public string? ProfilePicture { get; set; }

        public IFormFile? ProfilePictureFile { get; set; }


        // =========================================================
        // NAME
        // =========================================================

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }


        // =========================================================
        // DATE OF BIRTH
        // =========================================================

        public DateTime? DateOfBirth { get; set; }


        // =========================================================
        // GENDER
        // =========================================================

        public string? Gender { get; set; }


        // =========================================================
        // ADDRESS
        // =========================================================

        [StringLength(500)]
        public string? Address { get; set; }
    }
}
