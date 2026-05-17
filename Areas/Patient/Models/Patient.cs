using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Patient.Models
{
    [Table("patients")]
    public class Patient
    {
        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("date_of_birth")]
        public DateOnly DateOfBirth { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }


}
