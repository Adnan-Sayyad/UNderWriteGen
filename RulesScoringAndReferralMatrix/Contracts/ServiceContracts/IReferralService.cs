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
        Task<IEnumerable<ReferralResponseDto>> GetReferralsByAuthorityAsync(RequiredAuthority authority);
        Task<IEnumerable<ReferralResponseDto>> GetReferralsByAssignedToAsync(string userId);
        Task<ReferralResponseDto> CreateReferralAsync(CreateReferralDto dto);
        Task<ReferralResponseDto?> UpdateReferralAsync(Guid id, UpdateReferralDto dto);
        Task<ReferralResponseDto?> UpdateReferralStatusAsync(Guid id, UpdateReferralStatusDto dto);
    }
}
