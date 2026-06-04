using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Doctor.Models;

namespace Wellora.Areas.Doctor.Models
{
    [Table("doctors_schedule_history")]
    public class DoctorScheduleHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("old_schedule", TypeName = "json")]
        public string? OldSchedule { get; set; }

        [Column("new_schedule", TypeName = "json")]
        public string? NewSchedule { get; set; }
                
        [Column("action_type")]
        public string? ActionType { get; set; } // INSERT, UPDATE, DELETE

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
}