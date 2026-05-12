using ComplianceAuditAndQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplianceAuditAndQA.Data.Configurations
{
    public class AuthorityBreachConfiguration
        : IEntityTypeConfiguration<AuthorityBreach>
    {
        public void Configure(EntityTypeBuilder<AuthorityBreach> builder)
        {
            builder.ToTable("AuthorityBreaches");

            builder.HasKey(e => e.BreachId);
            builder.Property(e => e.BreachId).ValueGeneratedNever();

            builder.Property(e => e.SubmissionId).IsRequired();
            builder.Property(e => e.BreachType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Description).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(e => e.ApprovedBy).HasMaxLength(256);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            // ValueGeneratedNever() ensures EF Core always includes CreatedAt
            // in the INSERT statement using the value set by application code
            // (DateTime.UtcNow), rather than deferring to the DB default.
            // The GETUTCDATE() default remains in the schema as a safety net
            // for any direct SQL inserts.
            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()")
                   .ValueGeneratedNever();
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(e => e.SubmissionId).HasDatabaseName("IX_AuthorityBreaches_SubmissionId");
            builder.HasIndex(e => e.BreachType).HasDatabaseName("IX_AuthorityBreaches_BreachType");
            builder.HasIndex(e => e.Status).HasDatabaseName("IX_AuthorityBreaches_Status");
            builder.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_AuthorityBreaches_IsDeleted");
        }
    }
}
