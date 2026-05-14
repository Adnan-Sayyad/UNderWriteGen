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

        /// <summary>
        /// If a score already exists for the submission, updates its value/band/date and
        /// deletes any duplicate records. Otherwise creates a new record.
        /// Always returns exactly one record per submission.
        /// </summary>
        Task<RiskScore> UpsertScoreAsync(Guid submissionId, double scoreValue, Band band, string modelVersion);
    }
}
