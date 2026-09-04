using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Models;

namespace Wellora.Areas.Admin.Models
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("profile_picture")]
        public string? ProfilePicture { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("office_number")]
        public string? OfficeNumber { get; set; }

        [Column("office_official_number")]
        public string? OfficeOfficialNumber { get; set; }

        [Column("seniority")]
        public string? Seniority { get; set; }

        [Column("emergency_contact_name")]
        public string? EmergencyContactName { get; set; }

        [Column("emergency_contact_number")]
        public string? EmergencyContactNumber { get; set; }

        [Column("admin_type")]
        public string? AdminType { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}