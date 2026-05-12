using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Contracts.ServiceContracts
{
    public interface IReferralMatrixService
    {
        Task<IEnumerable<ReferralMatrixResponseDto>> GetAllMatricesAsync();

        Task<PagedResultDto<ReferralMatrixResponseDto>> GetMatricesPagedAsync(
            int page, int size,
            string? productLine, RequiredAuthority? authority, UWStatus? status);
        Task<ReferralMatrixResponseDto?> GetMatrixByIdAsync(Guid id);
        Task<IEnumerable<ReferralMatrixResponseDto>> GetMatricesByProductLineAsync(string productLine);
        Task<IEnumerable<ReferralMatrixResponseDto>> GetActiveMatricesAsync();
        Task<ReferralMatrixResponseDto> CreateMatrixAsync(CreateReferralMatrixDto dto);
        Task<ReferralMatrixResponseDto?> UpdateMatrixAsync(Guid id, UpdateReferralMatrixDto dto);
        Task<ReferralMatrixResponseDto?> UpdateMatrixStatusAsync(Guid id, UpdateMatrixStatusDto dto);
        Task<bool> DeleteMatrixAsync(Guid id);
    }
}
