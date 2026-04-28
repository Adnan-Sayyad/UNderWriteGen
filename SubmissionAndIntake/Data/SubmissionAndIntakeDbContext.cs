using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Models;

namespace SubmissionAndIntake.Data
{
    public class SubmissionAndIntakeDbContext : DbContext
    {
        public SubmissionAndIntakeDbContext(DbContextOptions<SubmissionAndIntakeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<CompletenessCheck> CompletenessChecks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Submission>().HasKey(s => s.SubmissionID);
            modelBuilder.Entity<Questionnaire>().HasKey(q => q.QID);
            modelBuilder.Entity<Attachment>().HasKey(a => a.AttachmentID);
            modelBuilder.Entity<CompletenessCheck>().HasKey(c => c.CheckID);
        }
    }
}
