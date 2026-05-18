using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Patient.Models;

namespace Wellora.Areas.Patient.Models
{
    [Table("ai_diagnosis")]
    public class AIDiagnosis
    {
        [Key]
        [Column("diagnosis_id")]
        public int DiagnosisId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("symptoms")]
        public string Symptoms { get; set; }

        [StringLength(255)]
        [Column("suggested_specialization")]
        public string SuggestedSpecialization { get; set; }

        [Column("confidence_score")]
        public decimal? ConfidenceScore { get; set; }

        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        // Navigation Property

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
    }
}