using IdentityAndAccessManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAndAccessManagement.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users"); // ✅ Fixed

            // ── Primary Key ───────────────────────────────────────
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever(); // Guid assigned by app

            // ── Columns ───────────────────────────────────────────
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("User");

            builder.Property(e => e.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(512);

            builder.Property(e => e.RefreshTokenExpiry)
                .IsRequired(false);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired(false);

            builder.Property(e => e.DeletedAt)
                .IsRequired(false);

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // ── Indexes ───────────────────────────────────────────
            builder.HasIndex(e => e.UserName)
                .IsUnique()
                .HasDatabaseName("IX_Users_UserName");

            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Users_IsDeleted");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Users_Status");
        }
    }
}