using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Patient.Models
{
    [Table("patients")]
    public class Patient
    {
        [Key]
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("full_name")]
        public string FullName { get; set; }

        [Column("date_of_birth")]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [StringLength(10)]
        [Column("gender")]
        public string Gender { get; set; }

        [StringLength(255)]
        [Column("address")]
        public string Address { get; set; }

        [StringLength(100)]
        [Column("emergency_contact_name")]
        public string EmergencyContactName { get; set; }

        [StringLength(20)]
        [Column("emergency_contact_phone")]
        public string EmergencyContactPhone { get; set; }

        [StringLength(5)]
        [Column("blood_group")]
        public string BloodGroup { get; set; }

        [Column("allergies")]
        public string Allergies { get; set; }

        [Column("medical_conditions")]
        public string MedicalConditions { get; set; }

        [Column("medications")]
        public string Medications { get; set; }

        [Column("primary_doctor_id")]
        public int? PrimaryDoctorId { get; set; }

        [StringLength(50)]
        [Column("preferred_language")]
        public string PreferredLanguage { get; set; }

        [StringLength(255)]
        [Column("profile_photo")]
        public string ProfilePhoto { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }


}
