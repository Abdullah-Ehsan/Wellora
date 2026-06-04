using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Areas.Doctor.Models;

namespace Wellora.Areas.Doctor.Models
{
    [Table("doctors_schedule")]
    public class DoctorSchedule
    {
        [Key]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("doctor_id")]
        public int DoctorId { get; set; }

        [Column("day_of_week")]
        public int DayOfWeek { get; set; } // 1=Mon ... 7=Sun

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("appointment_duration_min")]
        public int AppointmentDurationMin { get; set; }

        [Column("max_patients_per_day")]
        public int MaxPatientsPerDay { get; set; }

        [Column("buffer_time_min")] 
        public int BufferTimeMin { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
    }
}