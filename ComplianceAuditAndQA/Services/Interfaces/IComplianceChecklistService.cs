using ComplianceAuditAndQA.DTOs;

namespace ComplianceAuditAndQA.Services.Interfaces
{
    public interface IComplianceChecklistService
    {
        Task<IEnumerable<ComplianceChecklistDto>> GetAllAsync();
        Task<ComplianceChecklistDto>              GetByIdAsync(Guid checklistId);
        Task<ComplianceChecklistDto>              GetBySubmissionIdAsync(Guid submissionId);
        Task<ComplianceChecklistDto>              CreateAsync(CreateComplianceChecklistDto dto);
        Task<ComplianceChecklistDto>              UpdateAsync(Guid checklistId, UpdateComplianceChecklistDto dto);
        Task<ComplianceChecklistDto>              UpdateStatusAsync(Guid checklistId, UpdateChecklistStatusDto dto);
    }
}
