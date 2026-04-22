using IdentityAndAccessManagement.DTOs;

namespace IdentityAndAccessManagement.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDto>> GetAllAsync(Guid adminId);
        Task<AuditLogDto> GetByIdAsync(Guid adminId, Guid auditId);
        Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid adminId, Guid userId);
        Task<IEnumerable<AuditLogDto>> GetByResourceAsync(Guid adminId, string resource);
    }
}