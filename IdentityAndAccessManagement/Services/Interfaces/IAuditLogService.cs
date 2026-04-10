using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<PaginatedResultDto<AuditLogDto>> GetAllAsync(int page, int pageSize);
        Task<AuditLogDto> GetByIdAsync(Guid auditId);
        Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLogDto>> GetByResourceAsync(string resource);
    }
}