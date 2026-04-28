using Microsoft.EntityFrameworkCore;
using RiskDataAndEvidence.Domain;

namespace RiskDataAndEvidence.Persistence;

public class RiskDataDbContext : DbContext
{
    public RiskDataDbContext(DbContextOptions<RiskDataDbContext> options) : base(options) { }

    public DbSet<RiskProfile> RiskProfiles => Set<RiskProfile>();
    public DbSet<EvidenceRef> EvidenceRefs => Set<EvidenceRef>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RiskProfile>(e =>
        {
            e.HasKey(r => r.RiskID);
            e.Property(r => r.RiskType).HasMaxLength(50).IsRequired();
            e.Property(r => r.AttributesJSON).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(r => r.RiskNotes).HasMaxLength(2000);
            e.HasIndex(r => r.SubmissionID);
            e.HasIndex(r => r.RiskType);
        });

        modelBuilder.Entity<EvidenceRef>(e =>
        {
            e.HasKey(ev => ev.EvidenceID);
            e.Property(ev => ev.EvidenceType).HasMaxLength(50).IsRequired();
            e.Property(ev => ev.Provider).HasMaxLength(200).IsRequired();
            e.Property(ev => ev.ReferenceNo).HasMaxLength(100).IsRequired();
            e.Property(ev => ev.ResultJSON).HasColumnType("nvarchar(max)");
            e.Property(ev => ev.Status).HasMaxLength(20).IsRequired();
            e.HasIndex(ev => ev.SubmissionID);
            e.HasIndex(ev => new { ev.SubmissionID, ev.EvidenceType });
        });
    }
}
