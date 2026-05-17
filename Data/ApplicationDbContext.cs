using Microsoft.EntityFrameworkCore;
using Wellora.Areas.Patient.Models;
//using Wellora.Areas.Doctor.Models;
//using Wellora.Areas.Admin.Models;



namespace Wellora.Data
{
   
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Patient> Patients { get; set; }


        //public DbSet<Doctor> Doctors { get; set; }

        // public DbSet<Admin> Admins { get; set; }

    }
}
