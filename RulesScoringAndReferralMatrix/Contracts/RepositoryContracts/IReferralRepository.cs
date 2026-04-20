using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Contracts.RepositoryContracts
{
    public interface IReferralRepository
    {
        Task<IEnumerable<Referral>> GetAllAsync();
        Task<Referral?> GetByIdAsync(Guid id);
        Task<IEnumerable<Referral>> GetBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<Referral>> GetByStatusAsync(ReferralStatus status);
        Task<Referral> CreateAsync(Referral referral);
        Task<Referral?> UpdateAsync(Referral referral);
    }
}
