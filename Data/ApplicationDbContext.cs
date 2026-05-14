using Microsoft.EntityFrameworkCore;


namespace Wellora.Data
{
   
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Example table
        //public DbSet<Patient> Patients { get; set; }
    }
}
