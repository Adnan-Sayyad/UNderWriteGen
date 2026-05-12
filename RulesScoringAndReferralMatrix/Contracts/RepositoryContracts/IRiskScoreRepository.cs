using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Contracts.RepositoryContracts
{
    public interface IRiskScoreRepository
    {
        Task<IEnumerable<RiskScore>> GetAllAsync();
        Task<RiskScore?> GetByIdAsync(Guid id);
        Task<IEnumerable<RiskScore>> GetBySubmissionIdAsync(Guid submissionId);
        Task<(IEnumerable<RiskScore> Items, int Total)> GetPagedBySubmissionIdAsync(Guid submissionId, int page, int size);
        Task<RiskScore?> GetLatestBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<RiskScore>> GetByBandAsync(Band band);
        Task<(IEnumerable<RiskScore> Items, int Total)> GetPagedByBandAsync(Band band, int page, int size);
        Task<RiskScore> CreateAsync(RiskScore riskScore);
    }
}
