using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IRiskScoreService
    {
        Task<IEnumerable<RiskScoreResponseDto>> GetAllScoresAsync();
        Task<RiskScoreResponseDto?> GetScoreByIdAsync(Guid id);
        Task<IEnumerable<RiskScoreResponseDto>> GetScoresBySubmissionIdAsync(Guid submissionId);
        Task<PagedResultDto<RiskScoreResponseDto>> GetScoresBySubmissionPagedAsync(Guid submissionId, int page, int size);
        Task<RiskScoreResponseDto?> GetLatestScoreBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<RiskScoreResponseDto>> GetScoresByBandAsync(Band band);
        Task<PagedResultDto<RiskScoreResponseDto>> GetScoresByBandPagedAsync(Band band, int page, int size);
        Task<RiskScoreResponseDto> CreateScoreAsync(CreateRiskScoreDto dto);
        Task<RiskScoreResponseDto> CalculateScoreForSubmissionAsync(Guid submissionId);
        Task<RiskScoreResponseDto> UpsertScoreAsync(Guid submissionId, double scoreValue, Band band, string modelVersion);
    }
}
