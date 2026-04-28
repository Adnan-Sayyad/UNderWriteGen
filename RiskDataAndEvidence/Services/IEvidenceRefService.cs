using RiskDataAndEvidence.DTOs;

namespace RiskDataAndEvidence.Services;

public interface IEvidenceRefService
{
    Task<IEnumerable<EvidenceRefSummaryDto>> GetBySubmissionIdAsync(Guid submissionId);
    Task<EvidenceRefDetailDto?> GetByIdAsync(Guid evidenceId);
    Task<IEnumerable<EvidenceRefSummaryDto>> GetBySubmissionAndTypeAsync(Guid submissionId, string evidenceType);
    Task<EvidenceRefDetailDto> CreateAsync(CreateEvidenceRefDto dto);
    Task<EvidenceRefDetailDto?> UpdateAsync(Guid evidenceId, UpdateEvidenceRefDto dto);
    Task<EvidenceRefDetailDto?> UpdateStatusAsync(Guid evidenceId, UpdateEvidenceStatusDto dto);
}
