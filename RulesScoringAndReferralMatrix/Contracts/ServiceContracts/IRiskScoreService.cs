using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IRiskScoreService
    {
        Task<IEnumerable<RiskScoreResponseDto>> GetAllScoresAsync();
        Task<RiskScoreResponseDto?> GetScoreByIdAsync(Guid id);
        Task<IEnumerable<RiskScoreResponseDto>> GetScoresBySubmissionIdAsync(Guid submissionId);
        Task<RiskScoreResponseDto?> GetLatestScoreBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<RiskScoreResponseDto>> GetScoresByBandAsync(Band band);
        Task<RiskScoreResponseDto> CreateScoreAsync(CreateRiskScoreDto dto);
        Task<RiskScoreResponseDto> CalculateScoreForSubmissionAsync(Guid submissionId);
    }
}
