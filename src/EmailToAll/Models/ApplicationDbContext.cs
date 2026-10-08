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
        public DbSet<EmailSetup> EmailSetups { get; set; }
        public DbSet<EmailHistory> EmailHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailSetup>(entity =>
            {
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.Property(x => x.Email).HasMaxLength(256);
            });

            modelBuilder.Entity<EmailHistory>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.SentAt });
                entity.Property(x => x.FromEmail).HasMaxLength(256);
                entity.Property(x => x.Subject).HasMaxLength(500);
                entity.Property(x => x.Status).HasMaxLength(20);
                entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
            });
        }
    }
}
