using Microsoft.EntityFrameworkCore;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Data
{
    public class PolicyDbContext : DbContext
    {
        public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
            : base(options) { }

        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Renewal> Renewals => Set<Renewal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Policy
            modelBuilder.Entity<Policy>(e =>
            {
                e.HasKey(x => x.PolicyID);
                e.Property(x => x.PolicyID).ValueGeneratedOnAdd();
                e.Property(x => x.PolicyNumber).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.PolicyNumber).IsUnique();
                e.Property(x => x.ProductLine).IsRequired().HasMaxLength(50);
                e.Property(x => x.CoverageJSON).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
                e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");

                // A policy may have one renewal
                e.HasOne<Renewal>()
                 .WithOne()
                 .HasForeignKey<Renewal>(r => r.PolicyID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Renewal
            modelBuilder.Entity<Renewal>(e =>
            {
                e.HasKey(x => x.RenewalID);
                e.Property(x => x.RenewalID).ValueGeneratedOnAdd();
                e.Property(x => x.RenewalOfferJSON).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
                e.Property(x => x.OfferedDate).HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Offered");
            });
        }
    }
}
