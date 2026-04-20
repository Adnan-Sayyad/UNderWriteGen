using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IUWRuleService
    {
        Task<IEnumerable<UWRuleResponseDto>> GetAllRulesAsync();
        Task<UWRuleResponseDto?> GetRuleByIdAsync(Guid id);
        Task<IEnumerable<UWRuleResponseDto>> GetRulesByProductLineAsync(string productLine);
        Task<IEnumerable<UWRuleResponseDto>> GetActiveRulesAsync();
        Task<UWRuleResponseDto> CreateRuleAsync(CreateUWRuleDto dto);
        Task<UWRuleResponseDto?> UpdateRuleAsync(Guid id, UpdateUWRuleDto dto);
        Task<bool> DeleteRuleAsync(Guid id);
    }
}
