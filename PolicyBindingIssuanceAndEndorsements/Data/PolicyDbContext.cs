using Microsoft.EntityFrameworkCore;
using PolicyBindingIssuanceAndEndorsements.Models;

namespace PolicyBindingIssuanceAndEndorsements.Data
{
    public class PolicyDbContext : DbContext
    {
        public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
            : base(options) { }

        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Endorsement> Endorsements => Set<Endorsement>();
        public DbSet<Cancellation> Cancellations => Set<Cancellation>();
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

                // A policy may have many endorsements
                e.HasMany<Endorsement>()
                 .WithOne()
                 .HasForeignKey(en => en.PolicyID)
                 .OnDelete(DeleteBehavior.Cascade);

                // A policy may have one cancellation
                e.HasOne<Cancellation>()
                 .WithOne()
                 .HasForeignKey<Cancellation>(c => c.PolicyID)
                 .OnDelete(DeleteBehavior.Cascade);

                // A policy may have one renewal
                e.HasOne<Renewal>()
                 .WithOne()
                 .HasForeignKey<Renewal>(r => r.PolicyID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Endorsement
            modelBuilder.Entity<Endorsement>(e =>
            {
                e.HasKey(x => x.EndorsementID);
                e.Property(x => x.EndorsementID).ValueGeneratedOnAdd();
                e.Property(x => x.EndorsementType).IsRequired().HasMaxLength(50);
                e.Property(x => x.ChangesJSON).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
                e.Property(x => x.PremiumDelta).HasColumnType("decimal(18,2)");
                e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Proposed");
            });

            // Cancellation
            modelBuilder.Entity<Cancellation>(e =>
            {
                e.HasKey(x => x.CancellationID);
                e.Property(x => x.CancellationID).ValueGeneratedOnAdd();
                e.Property(x => x.CancelReason).IsRequired().HasMaxLength(500);
                e.Property(x => x.RefundPremium).HasColumnType("decimal(18,2)");
                e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Requested");
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
