using Microsoft.EntityFrameworkCore;
using NotificationsAndAlerts.Models.Entities;

namespace NotificationsAndAlerts.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");
                entity.HasIndex(n => n.UserID);
                entity.HasIndex(n => n.Status);
                entity.HasIndex(n => n.Category);
            });

            // Seed sample data
            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    NotificationID = 1,
                    UserID = "USR-001",
                    Message = "Submission SUB-2026-0042 has been referred for your review.",
                    Category = "Referral",
                    Status = "Unread",
                    CreatedDate = new DateTime(2026, 1, 2, 9, 30, 0)
                },
                new Notification
                {
                    NotificationID = 2,
                    UserID = "USR-002",
                    Message = "Quote QT-2026-0099 has been generated and is awaiting customer acceptance.",
                    Category = "Quote",
                    Status = "Read",
                    CreatedDate = new DateTime(2026, 1, 3, 11, 15, 0)
                },
                new Notification
                {
                    NotificationID = 3,
                    UserID = "USR-001",
                    Message = "SLA breach warning: case CASE-2026-0007 nearing 24-hour deadline.",
                    Category = "SLA",
                    Status = "Unread",
                    CreatedDate = new DateTime(2026, 1, 4, 8, 0, 0)
                }
            );
        }
    }
}
