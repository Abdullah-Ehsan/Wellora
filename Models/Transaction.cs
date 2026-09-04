using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Patient.Models;

namespace Wellora.Models
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("appointment_id")]
        public int AppointmentId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } = null!;

        [Column("stripe_session_id")]
        public string? StripeSessionId { get; set; }

        [Column("stripe_payment_intent_id")]
        public string? StripePaymentIntentId { get; set; }

        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        // Navigation Properties

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}
