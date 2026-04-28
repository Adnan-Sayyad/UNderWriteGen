using ComplianceAuditAndQA.Data.Configurations;
using ComplianceAuditAndQA.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Data
{
    public class ComplianceDbContext : DbContext
    {
        public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options)
            : base(options) { }

        public DbSet<ComplianceChecklist> ComplianceChecklists => Set<ComplianceChecklist>();
        public DbSet<AuthorityBreach>     AuthorityBreaches    => Set<AuthorityBreach>();
        public DbSet<ExceptionLog>        ExceptionLogs        => Set<ExceptionLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new ComplianceChecklistConfiguration());
            modelBuilder.ApplyConfiguration(new AuthorityBreachConfiguration());
            modelBuilder.ApplyConfiguration(new ExceptionLogConfiguration());
        }
    }
}
