using ComplianceAuditAndQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplianceAuditAndQA.Data.Configurations
{
    public class ExceptionLogConfiguration
        : IEntityTypeConfiguration<ExceptionLog>
    {
        public void Configure(EntityTypeBuilder<ExceptionLog> builder)
        {
            builder.ToTable("ExceptionLogs");

            builder.HasKey(e => e.ExceptionId);
            builder.Property(e => e.ExceptionId).ValueGeneratedNever();

            builder.Property(e => e.SubmissionId).IsRequired();
            builder.Property(e => e.Category).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Details).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(e => e.LoggedDate)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()")
                   .ValueGeneratedNever();
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()")
                   .ValueGeneratedNever();
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(e => e.SubmissionId).HasDatabaseName("IX_ExceptionLogs_SubmissionId");
            builder.HasIndex(e => e.Category).HasDatabaseName("IX_ExceptionLogs_Category");
            builder.HasIndex(e => e.Status).HasDatabaseName("IX_ExceptionLogs_Status");
            builder.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_ExceptionLogs_IsDeleted");
        }
    }
}
