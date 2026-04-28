using Microsoft.EntityFrameworkCore;
using RiskDataAndEvidence.Domain;
using RiskDataAndEvidence.Persistence;

namespace RiskDataAndEvidence.Repositories;

public class EvidenceRefRepository : IEvidenceRefRepository
{
    private readonly RiskDataDbContext _db;

    public EvidenceRefRepository(RiskDataDbContext db) => _db = db;

    public async Task<IEnumerable<EvidenceRef>> GetBySubmissionIdAsync(Guid submissionId)
        => await _db.EvidenceRefs
            .Where(e => e.SubmissionID == submissionId)
            .OrderBy(e => e.EvidenceType)
            .ToListAsync();

    public async Task<EvidenceRef?> GetByIdAsync(Guid evidenceId)
        => await _db.EvidenceRefs.FindAsync(evidenceId);

    public async Task<IEnumerable<EvidenceRef>> GetBySubmissionAndTypeAsync(Guid submissionId, string evidenceType)
        => await _db.EvidenceRefs
            .Where(e => e.SubmissionID == submissionId && e.EvidenceType == evidenceType)
            .ToListAsync();

    public async Task AddAsync(EvidenceRef evidence)
        => await _db.EvidenceRefs.AddAsync(evidence);

    public Task UpdateAsync(EvidenceRef evidence)
    {
        _db.EvidenceRefs.Update(evidence);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
