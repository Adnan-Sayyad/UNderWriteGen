using IdentityAndAccessManagement.Data;
using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Models;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndAccessManagement.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationUserDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogService(
            ApplicationUserDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── GET /api/audit-logs?adminId=... ───────────────────────
        public async Task<IEnumerable<AuditLogDto>> GetAllAsync(Guid adminId)
        {
            await ValidateAdminAsync(adminId);

            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return logs.Select(MapToDto);
        }

        // ── GET /api/audit-logs/{auditId}?adminId=... ────────────
        public async Task<AuditLogDto> GetByIdAsync(Guid adminId, Guid auditId)
        {
            await ValidateAdminAsync(adminId);

            var log = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == auditId && !a.IsDeleted)
                ?? throw new KeyNotFoundException("Audit log not found.");

            return MapToDto(log);
        }

        // ── GET /api/audit-logs/user/{userId}?adminId=... ─────────
        public async Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid adminId, Guid userId)
        {
            await ValidateAdminAsync(adminId);

            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (!logs.Any())
                throw new KeyNotFoundException("No audit logs found for this user.");

            return logs.Select(MapToDto);
        }

        // ── GET /api/audit-logs/resource/{resource}?adminId=... ──
        public async Task<IEnumerable<AuditLogDto>> GetByResourceAsync(Guid adminId, string resource)
        {
            await ValidateAdminAsync(adminId);

            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be empty. Valid values are: 'Auth', 'UserManagement'.");

            // SQL Server collation is case-insensitive by default — plain equality works fine
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Resource == resource.Trim() && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (!logs.Any())
                throw new KeyNotFoundException(
                    $"No audit logs found for resource '{resource}'. " +
                    "Valid values are: 'Auth' (login/register/password events) or " +
                    "'UserManagement' (update/delete/status change events).");

            return logs.Select(MapToDto);
        }

        // ── Private: Validate Admin ────────────────────────────────
        private async Task ValidateAdminAsync(Guid adminId)
        {
            // ── Step 1: Account must exist ────────────────────────
            var admin = await _userManager.FindByIdAsync(adminId.ToString())
                ?? throw new UnauthorizedAccessException(
                    $"No account found with ID '{adminId}'. Please provide a valid admin ID.");

            // ── Step 2: Account must not be deleted ───────────────
            if (admin.IsDeleted)
                throw new UnauthorizedAccessException(
                    "This account has been deactivated. Please contact the administrator.");

            // ── Step 3: Account must not be locked or disabled ────
            if (admin.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "This account is locked. Please contact the administrator to unlock it.");

            if (admin.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "This account has been disabled. Please contact the administrator.");

            // ── Step 4: Must be logged in (RefreshToken present) ──
            if (string.IsNullOrEmpty(admin.RefreshToken))
                throw new UnauthorizedAccessException(
                    "You are not logged in. Please login first to access audit logs.");

            // ── Step 5: Must have Admin role ──────────────────────
            var roles = await _userManager.GetRolesAsync(admin);
            if (!roles.Contains("Admin"))
                throw new UnauthorizedAccessException(
                    "Access denied. Only logged in Admins are authorized to view audit logs.");
        }

        // ── Private: Map to DTO ───────────────────────────────────
        private static AuditLogDto MapToDto(AuditLogs log)
        {
            return new AuditLogDto
            {
                Id        = log.Id,
                UserId    = log.UserId,
                Email     = log.Email,
                Action    = log.Action,
                Resource  = log.Resource,
                CreatedAt = log.CreatedAt,
                Metadata  = log.Metadata,
                IsDeleted = log.IsDeleted
            };
        }
    }
}
