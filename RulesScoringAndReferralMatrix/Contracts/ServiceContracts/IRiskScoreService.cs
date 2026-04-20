using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IRiskScoreService
    {
        Task<IEnumerable<RiskScoreResponseDto>> GetAllScoresAsync();
        Task<RiskScoreResponseDto?> GetScoreByIdAsync(Guid id);
        Task<IEnumerable<RiskScoreResponseDto>> GetScoresBySubmissionIdAsync(Guid submissionId);
        Task<RiskScoreResponseDto> CreateScoreAsync(CreateRiskScoreDto dto);
    }
}
