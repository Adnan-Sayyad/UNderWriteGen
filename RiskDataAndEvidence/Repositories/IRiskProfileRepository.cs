using RiskDataAndEvidence.Domain;

namespace RiskDataAndEvidence.Repositories;

public interface IRiskProfileRepository
{
    Task<IEnumerable<RiskProfile>> GetAllAsync();
    Task<RiskProfile?> GetByIdAsync(Guid riskId);
    Task<RiskProfile?> GetBySubmissionIdAsync(Guid submissionId);
    Task<IEnumerable<RiskProfile>> GetByRiskTypeAsync(string riskType);
    Task AddAsync(RiskProfile profile);
    Task UpdateAsync(RiskProfile profile);
    Task DeleteAsync(RiskProfile profile);
    Task SaveChangesAsync();
}
