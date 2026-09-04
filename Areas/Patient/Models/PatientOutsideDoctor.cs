using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Patient.Models
{
    [Table("patient_outside_doctors")]
    public class PatientOutsideDoctor
    {
        [Key]
        [Column("patient_outside_doctors_id")]
        public int PatientOutsideDoctorsId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("outside_doctor_id")]
        public int OutsideDoctorId { get; set; }

        [StringLength(100)]
        [Column("relationship_type")]
        public string? RelationshipType { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }


        // Patient relationship
        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;


        // Outside doctor relationship
        [ForeignKey(nameof(OutsideDoctorId))]
        public virtual OutsideDoctor OutsideDoctor { get; set; } = null!;
    }
}
