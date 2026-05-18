using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Patient.Models;
using Wellora.Models;

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

        [StringLength(20)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; }

        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; }

        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        // Navigation Properties

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
    }
}