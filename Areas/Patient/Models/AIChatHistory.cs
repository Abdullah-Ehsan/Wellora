using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Patient.Models;

namespace Wellora.Areas.Patient.Models
{
    [Table("ai_chat_history")]
    public class AIChatHistory
    {
        [Key]
        [Column("chat_id")]
        public int ChatId { get; set; }

        [Column("patient_id")]
        public int PatientId { get; set; }

        [Column("user_message")]
        public string UserMessage { get; set; }

        [Column("ai_response")]
        public string AIResponse { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        // Navigation Property

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
    }
}