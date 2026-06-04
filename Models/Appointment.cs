using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Patient.Models;

namespace Wellora.Models
{
    [Table("appointments")]
    public class Appointment
    {
        [Key]
        [Column("appointment_id")]
        public int AppointmentId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Required]
        [Column("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [Column("scheduled_status")]
        public string? Status { get; set; }

        [StringLength(20)]
        [Column("payment_status")]
        public string? PaymentStatus { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("consultation_fee")]
        public decimal ConsultationFee { get; set; }

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
}