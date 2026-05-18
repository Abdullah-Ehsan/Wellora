using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Doctor.Models;

namespace Wellora.Areas.Doctor.Models
{
    [Table("doctor_schedule")]
    public class DoctorSchedule
    {
        [Key]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [StringLength(3)]
        [Column("day_of_week")]
        public string DayOfWeek { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        // Navigation Property (assuming Doctor model exists)
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }
}