using RiskDataAndEvidence.DTOs;

namespace RiskDataAndEvidence.Services;

public interface IRiskProfileService
{
    Task<IEnumerable<RiskProfileSummaryDto>> GetAllAsync();
    Task<RiskProfileDetailDto?> GetByIdAsync(Guid riskId);
    Task<RiskProfileDetailDto?> GetBySubmissionIdAsync(Guid submissionId);
    Task<IEnumerable<RiskProfileSummaryDto>> GetByRiskTypeAsync(string riskType);
    Task<RiskProfileDetailDto> CreateAsync(CreateRiskProfileDto dto);
    Task<RiskProfileDetailDto?> UpdateAsync(Guid riskId, UpdateRiskProfileDto dto);
    Task<bool> DeleteAsync(Guid riskId);
}
