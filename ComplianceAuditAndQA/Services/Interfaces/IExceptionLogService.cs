using ComplianceAuditAndQA.DTOs;

namespace ComplianceAuditAndQA.Services.Interfaces
{
    public interface IExceptionLogService
    {
        Task<IEnumerable<ExceptionLogDto>> GetAllAsync();
        Task<ExceptionLogDto>              GetByIdAsync(Guid exceptionId);
        Task<IEnumerable<ExceptionLogDto>> GetBySubmissionIdAsync(Guid submissionId);
        Task<IEnumerable<ExceptionLogDto>> GetByCategoryAsync(string category);
        Task<ExceptionLogDto>              CreateAsync(CreateExceptionLogDto dto);
        Task<ExceptionLogDto>              UpdateAsync(Guid exceptionId, UpdateExceptionLogDto dto);
        Task<ExceptionLogDto>              UpdateStatusAsync(Guid exceptionId, UpdateExceptionStatusDto dto);
    }
}
