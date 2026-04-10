using IdentityAndAccessManagement.Data;
using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Models;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndAccessManagement.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationUserDbContext _context;

        public AuditLogService(ApplicationUserDbContext context)
        {
            _context = context;
        }

        // ── GET /api/audit-logs?page=1&pageSize=10 ────────────────
        public async Task<PaginatedResultDto<AuditLogDto>> GetAllAsync(int page, int pageSize)
        {
            // ── Sanitize pagination values ────────────────────────
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            // ── Build base query ──────────────────────────────────
            // Uses IX_AuditLogs_CreatedAt index for ordering
            var query = _context.AuditLogs
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt);

            // ── Count before pagination ───────────────────────────
            var totalCount = await query.CountAsync();

            // ── Apply pagination ──────────────────────────────────
            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResultDto<AuditLogDto>
            {
                Data = logs.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── GET /api/audit-logs/{auditId} ─────────────────────────
        public async Task<AuditLogDto> GetByIdAsync(Guid auditId)
        {
            // Uses PK index (AuditId) for direct lookup
            var log = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == auditId && !a.IsDeleted)
                ?? throw new KeyNotFoundException("Audit log not found.");

            return MapToDto(log);
        }

        // ── GET /api/audit-logs/user/{userId} ─────────────────────
        public async Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid userId)
        {
            // Uses IX_AuditLogs_UserId_CreatedAt composite index
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (!logs.Any())
                throw new KeyNotFoundException("No audit logs found for this user.");

            return logs.Select(MapToDto);
        }

        // ── GET /api/audit-logs/resource/{resource} ───────────────
        public async Task<IEnumerable<AuditLogDto>> GetByResourceAsync(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be empty.");

            // Uses IX_AuditLogs_Resource index
            // EF.Functions.Like avoids ToLower() which bypasses indexes
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => EF.Functions.Like(a.Resource, resource) && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (!logs.Any())
                throw new KeyNotFoundException("No audit logs found for this resource.");

            return logs.Select(MapToDto);
        }

        // ── Private: Map to DTO ───────────────────────────────────
        private static AuditLogDto MapToDto(AuditLogs log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Email = log.Email,
                Action = log.Action,
                Resource = log.Resource,
                CreatedAt = log.CreatedAt,
                Metadata = log.Metadata,
                IsDeleted = log.IsDeleted
            };
        }
    }
}