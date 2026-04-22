using Microsoft.EntityFrameworkCore;
using UnderwritingWorkflowAndDecisions.Models;

namespace UnderwritingWorkflowAndDecisions.Data
{
    public class UWWorkflowDbContext : DbContext
    {
        public UWWorkflowDbContext(DbContextOptions<UWWorkflowDbContext> options)
            : base(options) { }

        public DbSet<UWNote> UWNotes => Set<UWNote>();
        public DbSet<UWDecision> UWDecisions => Set<UWDecision>();
        public DbSet<Subjectivity> Subjectivities => Set<Subjectivity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UWNote
            modelBuilder.Entity<UWNote>(e =>
            {
                e.HasKey(x => x.NoteID);
                e.Property(x => x.NoteID).ValueGeneratedOnAdd();
                e.Property(x => x.NoteText).IsRequired().HasMaxLength(2000);
                e.Property(x => x.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // UWDecision
            modelBuilder.Entity<UWDecision>(e =>
            {
                e.HasKey(x => x.DecisionID);
                e.Property(x => x.DecisionID).ValueGeneratedOnAdd();
                e.Property(x => x.Decision).IsRequired().HasMaxLength(50);
                e.Property(x => x.Reason).HasMaxLength(1000);
                e.Property(x => x.DecidedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Subjectivity
            modelBuilder.Entity<Subjectivity>(e =>
            {
                e.HasKey(x => x.SubjectivityID);
                e.Property(x => x.SubjectivityID).ValueGeneratedOnAdd();
                e.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
            });
        }
    }
}
