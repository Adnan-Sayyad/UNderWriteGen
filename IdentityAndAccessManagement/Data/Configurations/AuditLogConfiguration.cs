using IdentityAndAccessManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAndAccessManagement.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogs>
    {
        public void Configure(EntityTypeBuilder<AuditLogs> builder) 
        {
            builder.ToTable("AuditLogs");

            // ── Primary Key ───────────────────────────────────────
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("AuditId")
                .ValueGeneratedNever();

            // ── Foreign Key → Users table ─────────────────────────
            builder.Property(e => e.UserId)
                .IsRequired(false);

            builder.HasOne<ApplicationUser>(a => a.User)
                .WithMany(nameof(ApplicationUser.AuditLogs))
                .HasForeignKey(a => a.UserId)
                .HasConstraintName("FK_AuditLogs_Users_UserId")
                .OnDelete(DeleteBehavior.SetNull);

            // ── Columns ───────────────────────────────────────────
            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Action)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Resource)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // ── Indexes ───────────────────────────────────────────
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_AuditLogs_CreatedAt");

            builder.HasIndex(e => e.Resource)
                .HasDatabaseName("IX_AuditLogs_Resource");

            builder.HasIndex(e => new { e.UserId, e.CreatedAt })
                .HasDatabaseName("IX_AuditLogs_UserId_CreatedAt");
        }
    }
}