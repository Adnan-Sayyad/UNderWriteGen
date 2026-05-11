using IdentityAndAccessManagement.Data;
using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Models;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityAndAccessManagement.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationUserDbContext _context;

        public UserService(
            UserManager<ApplicationUser> userManager,
            ApplicationUserDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ── GET /api/users ────────────────────────────────────────
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(Guid adminId)
        {
            await ValidateAdminAsync(adminId, requireAdminOnly: false);

            var users = _userManager.Users
                .Where(u => !u.IsDeleted)
                .ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(MapToDto(user, roles));
            }

            return userDtos;
        }

        // ── GET /api/users/me ─────────────────────────────────────
        public async Task<UserDto> GetMyProfileAsync(Guid userId)
        {
            await ValidateSelfAsync(userId);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    "This account has been deactivated.");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── GET /api/users/{userId} ───────────────────────────────
        public async Task<UserDto> GetUserByIdAsync(Guid adminId, Guid userId)
        {
            await ValidateAdminAsync(adminId, requireAdminOnly: false);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── PUT /api/users/{userId} ───────────────────────────────
        public async Task<UserDto> UpdateUserAsync(Guid adminId, Guid userId, UpdateUserDto dto)
        {
            await ValidateAdminAsync(adminId, requireAdminOnly: true);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    "This account has been deactivated and cannot be updated.");

            user.FirstName   = dto.FirstName;
            user.LastName    = dto.LastName;
            user.Email       = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt   = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await LogAuditAsync(user, "UserUpdated", "UserManagement",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}, Phone: {user.PhoneNumber ?? "—"}");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── PATCH /api/users/{userId}/status ─────────────────────
        public async Task UpdateUserStatusAsync(Guid adminId, Guid userId, UpdateUserStatusDto dto)
        {
            await ValidateAdminAsync(adminId, requireAdminOnly: true);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    "This account has been deactivated and cannot be updated.");

            var previousStatus = user.Status;
            user.Status    = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            // ── Lock / Disable: invalidate session ────────────────
            if (dto.Status == "Locked" || dto.Status == "Disabled")
            {
                user.RefreshToken = null;
                await _userManager.SetLockoutEndDateAsync(
                    user, DateTimeOffset.UtcNow.AddYears(100));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user,
                $"StatusChanged: {previousStatus} → {dto.Status}",
                "UserManagement",
                $"User: {user.FirstName} {user.LastName} ({user.Email}), PreviousStatus: {previousStatus}, NewStatus: {dto.Status}");
        }

        // ── DELETE /api/users/{userId} ────────────────────────────
        public async Task DeleteUserAsync(Guid adminId, Guid userId)
        {
            await ValidateAdminAsync(adminId, requireAdminOnly: true);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    $"No account found with ID '{userId}'. Please provide a valid user ID.");

            // ── Soft delete ───────────────────────────────────────
            user.IsDeleted    = true;
            user.DeletedAt    = DateTime.UtcNow;
            user.UpdatedAt    = DateTime.UtcNow;
            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "UserDeleted", "UserManagement",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}, DeletedAt: {DateTime.UtcNow:u}");
        }

        // ── Private: Validate Admin ───────────────────────────────
        private async Task ValidateAdminAsync(Guid adminId, bool requireAdminOnly)
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
                    "You are not logged in. Please login first to access user management.");

            // ── Step 5: Role check ────────────────────────────────
            var roles = await _userManager.GetRolesAsync(admin);

            if (requireAdminOnly && !roles.Contains("Admin"))
                throw new UnauthorizedAccessException(
                    "Access denied. Only logged in Admins can perform this action.");

            if (!requireAdminOnly && !roles.Contains("Admin"))
                throw new UnauthorizedAccessException(
                    "Access denied. Only Admins can view users.");
        }

        // ── Private: Validate Self (no role check, any active user) ──
        private async Task ValidateSelfAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UnauthorizedAccessException(
                    $"No account found with ID '{userId}'.");

            if (user.IsDeleted)
                throw new UnauthorizedAccessException(
                    "This account has been deactivated. Please contact the administrator.");

            if (user.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "This account is locked. Please contact the administrator to unlock it.");

            if (user.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "This account has been disabled. Please contact the administrator.");
        }

        // ── Private: Map to DTO ───────────────────────────────────
        private static UserDto MapToDto(ApplicationUser user, IList<string> roles)
        {
            return new UserDto
            {
                Id          = user.Id,
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                UserName    = user.UserName ?? string.Empty,
                Email       = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role        = string.Join(", ", roles),
                Status      = user.Status,
                CreatedAt   = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc),
                UpdatedAt   = user.UpdatedAt.HasValue
                              ? DateTime.SpecifyKind(user.UpdatedAt.Value, DateTimeKind.Utc)
                              : null
            };
        }

        // ── Private: Audit Logger ─────────────────────────────────
        private async Task LogAuditAsync(
            ApplicationUser user, string action, string resource, string? metadata = null)
        {
            var audit = new AuditLogs
            {
                UserId    = user.Id,
                Email     = user.Email ?? string.Empty,
                Action    = action,
                Resource  = resource,
                Metadata  = metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(audit);
            await _context.SaveChangesAsync();
        }
    }
}
