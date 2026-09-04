using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Patient.Models
{
    [Table("outside_doctors")]
    public class OutsideDoctor
    {
        [Key]
        [Column("outside_doctor_id")]
        public int OutsideDoctorId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("doctor_name")]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("doctor_specialty")]
        public string? DoctorSpecialty { get; set; }

        [StringLength(200)]
        [Column("hospital_name")]
        public string? HospitalName { get; set; }

        [StringLength(100)]
        [Column("hospital_city")]
        public string? HospitalCity { get; set; }

        [StringLength(100)]
        [Column("hospital_country")]
        public string? HospitalCountry { get; set; }

        [StringLength(30)]
        [Column("doctor_phone")]
        public string? DoctorPhone { get; set; }

        [StringLength(150)]
        [Column("doctor_email")]
        public string? DoctorEmail { get; set; }

        [StringLength(255)]
        [Column("doctor_photo")]
        public string? DoctorPhoto { get; set; }


        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }


        // Patients connected to this outside doctor
        public virtual ICollection<PatientOutsideDoctor> PatientOutsideDoctors { get; set; }
            = new List<PatientOutsideDoctor>();
    }
}
