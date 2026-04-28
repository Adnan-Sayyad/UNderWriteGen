using ComplianceAuditAndQA.DTOs;

namespace ComplianceAuditAndQA.Services.Interfaces
{
    public interface IAuthorityBreachService
    {
        Task<IEnumerable<AuthorityBreachDto>> GetAllAsync();
        Task<AuthorityBreachDto>              GetByIdAsync(Guid breachId);
        Task<IEnumerable<AuthorityBreachDto>> GetBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<AuthorityBreachDto>> GetByBreachTypeAsync(string breachType);
        Task<AuthorityBreachDto>              CreateAsync(CreateAuthorityBreachDto dto);
        Task<AuthorityBreachDto>              UpdateAsync(Guid breachId, UpdateAuthorityBreachDto dto);
        Task<AuthorityBreachDto>              UpdateStatusAsync(Guid breachId, UpdateBreachStatusDto dto);
    }
}
