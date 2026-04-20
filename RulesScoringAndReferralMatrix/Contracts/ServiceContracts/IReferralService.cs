using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IReferralService
    {
        Task<IEnumerable<ReferralResponseDto>> GetAllReferralsAsync();
        Task<ReferralResponseDto?> GetReferralByIdAsync(Guid id);
        Task<IEnumerable<ReferralResponseDto>> GetReferralsBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<ReferralResponseDto>> GetReferralsByStatusAsync(ReferralStatus status);
        Task<ReferralResponseDto> CreateReferralAsync(CreateReferralDto dto);
        Task<ReferralResponseDto?> UpdateReferralAsync(Guid id, UpdateReferralDto dto);
    }
}
