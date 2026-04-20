using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Contracts.RepositoryContracts
{
    public interface IRiskScoreRepository
    {
        Task<IEnumerable<RiskScore>> GetAllAsync();
        Task<RiskScore?> GetByIdAsync(Guid id);
        Task<IEnumerable<RiskScore>> GetBySubmissionIdAsync(Guid submissionId);
        Task<RiskScore> CreateAsync(RiskScore riskScore);
    }
}
