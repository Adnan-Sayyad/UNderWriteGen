using RiskDataAndEvidence.Domain;

namespace RiskDataAndEvidence.Repositories;

public interface IEvidenceRefRepository
{
    Task<IEnumerable<EvidenceRef>> GetBySubmissionIdAsync(Guid submissionId);
    Task<EvidenceRef?> GetByIdAsync(Guid evidenceId);
    Task<IEnumerable<EvidenceRef>> GetBySubmissionAndTypeAsync(Guid submissionId, string evidenceType);
    Task AddAsync(EvidenceRef evidence);
    Task UpdateAsync(EvidenceRef evidence);
    Task SaveChangesAsync();
}
