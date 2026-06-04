using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Doctor.Models;

namespace Wellora.Areas.Doctor.Models
{
    [Table("doctors_breaks")]
    public class DoctorBreak
    {
        [Key]
        [Column("break_id")]
        public int BreakId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("day_of_week")]
        public int DayOfWeek { get; set; }

        [Column("break_start")]
        public TimeSpan BreakStart { get; set; }

        [Column("break_end")]
        public TimeSpan BreakEnd { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
}