using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Models;

namespace RulesScoringAndReferralMatrix.Contracts.RepositoryContracts
{
    public interface IUWRuleRepository
    {
        Task<IEnumerable<UWRule>> GetAllAsync();
        Task<UWRule?> GetByIdAsync(Guid id);
        Task<IEnumerable<UWRule>> GetByProductLineAsync(string productLine);
        Task<IEnumerable<UWRule>> GetActiveRulesAsync();
        Task<UWRule> CreateAsync(UWRule rule);
        Task<UWRule?> UpdateAsync(UWRule rule);
        Task<bool> DeleteAsync(Guid id);
    }
}
