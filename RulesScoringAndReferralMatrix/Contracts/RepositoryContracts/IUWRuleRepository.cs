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
        Task<IEnumerable<UWRule>> GetBySeverityAsync(Severity severity);
        Task<UWRule> CreateAsync(UWRule rule);
        Task<UWRule?> UpdateAsync(UWRule rule);
        Task<UWRule?> UpdateStatusAsync(Guid id, UWStatus status);
        Task<bool> DeleteAsync(Guid id);
    }
}
