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

        }
    }
}
