using ComplianceAuditAndQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplianceAuditAndQA.Data.Configurations
{
    public class ComplianceChecklistConfiguration
        : IEntityTypeConfiguration<ComplianceChecklist>
    {
        public void Configure(EntityTypeBuilder<ComplianceChecklist> builder)
        {
            builder.ToTable("ComplianceChecklists");

            builder.HasKey(e => e.ChecklistId);
            builder.Property(e => e.ChecklistId).ValueGeneratedNever();

            builder.Property(e => e.SubmissionId).IsRequired();
            builder.Property(e => e.ItemsJson).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(e => e.CompletedBy).HasMaxLength(256);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(e => e.SubmissionId).HasDatabaseName("IX_ComplianceChecklists_SubmissionId");
            builder.HasIndex(e => e.Status).HasDatabaseName("IX_ComplianceChecklists_Status");
            builder.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_ComplianceChecklists_IsDeleted");
        }
    }
}
