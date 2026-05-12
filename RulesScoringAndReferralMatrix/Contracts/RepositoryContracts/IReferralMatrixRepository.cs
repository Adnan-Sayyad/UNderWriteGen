using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Contracts.RepositoryContracts
{
    public interface IReferralMatrixRepository
    {
        Task<IEnumerable<ReferralMatrix>> GetAllAsync();

        Task<(IEnumerable<ReferralMatrix> Items, int Total)> GetPagedAsync(
            int page, int size,
            string? productLine, RequiredAuthority? authority, UWStatus? status);

        Task<ReferralMatrix?> GetByIdAsync(Guid id);
        Task<IEnumerable<ReferralMatrix>> GetByProductLineAsync(string productLine);
        Task<IEnumerable<ReferralMatrix>> GetActiveMatricesAsync();
        Task<ReferralMatrix> CreateAsync(ReferralMatrix matrix);
        Task<ReferralMatrix?> UpdateAsync(ReferralMatrix matrix);
        Task<ReferralMatrix?> UpdateStatusAsync(Guid id, UWStatus status);
        Task<bool> DeleteAsync(Guid id);
    }
}
