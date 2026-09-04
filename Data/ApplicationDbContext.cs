using Microsoft.EntityFrameworkCore;
using Wellora.Models;
using Wellora.Areas.Patient.Models;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Admin.Models;




namespace Wellora.Data
{
   
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //Public Models
        public DbSet<User> Users { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Transaction> Transactions { get; set; }


        // Patient Models
        public DbSet<Patient> Patients { get; set; }

        public DbSet<AIDiagnosis> AIDiagnosises { get; set; }

        public DbSet<AIChatHistory> AIChatHistories { get; set; }

        public DbSet<OutsideDoctor> OutsideDoctors { get; set; }
        public DbSet<PatientOutsideDoctor> PatientOutsideDoctors { get; set; }



        //Doctor Models
        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<DoctorBreak> DoctorBreaks { get; set; }

        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

        public DbSet<DoctorScheduleHistory> DoctorScheduleHistorys { get; set; }



        //Admin Models
        public DbSet<Admin> Admins { get; set; }

    }
}
