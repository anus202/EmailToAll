using Microsoft.EntityFrameworkCore;

namespace EmailToAll.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

       public DbSet<Signup> Signups { get; set; }
       public DbSet<OtpVerification> OtpVerifications { get; set; }
    }
}
