using HomeBuyingApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeBuyingApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Property> Properties { get; set; }
        public DbSet<MortgageScenario> MortgageScenarios { get; set; }
        public DbSet<ViewingAppointment> Viewings { get; set; }
        public DbSet<PropertyAttachment> Attachments { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=homebuying.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if needed, though EF Core conventions handle most
            modelBuilder.Entity<Property>()
                .HasMany(p => p.MortgageScenarios)
                .WithOne(m => m.Property)
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Viewings)
                .WithOne(v => v.Property)
                .HasForeignKey(v => v.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
