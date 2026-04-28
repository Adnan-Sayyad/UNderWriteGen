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
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(e => e.SubmissionId).HasDatabaseName("IX_AuthorityBreaches_SubmissionId");
            builder.HasIndex(e => e.BreachType).HasDatabaseName("IX_AuthorityBreaches_BreachType");
            builder.HasIndex(e => e.Status).HasDatabaseName("IX_AuthorityBreaches_Status");
            builder.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_AuthorityBreaches_IsDeleted");
        }
    }
}
