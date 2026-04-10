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
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationUserDbContext _context;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationUserDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ── GET /api/users ────────────────────────────────────────
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
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

        // ── GET /api/users/{userId} ───────────────────────────────
        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── POST /api/users ───────────────────────────────────────
        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            // ── Step 1: Check email ───────────────────────────────
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already in use.");

            // ── Step 2: Validate role exists ──────────────────────
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                throw new InvalidOperationException(
                    $"Role '{dto.Role}' does not exist.");

            // ── Step 3: Create user ───────────────────────────────
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // ── Step 4: Assign role in UserRoles table ────────────
            await AssignRolesInternalAsync(user, new List<string> { dto.Role });

            // ── Step 5: Audit log ─────────────────────────────────
            await LogAuditAsync(user,
                $"UserCreated with Role: {dto.Role}",
                "UserManagement");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── PUT /api/users/{userId} ───────────────────────────────
        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await LogAuditAsync(user, "UserUpdated", "UserManagement");

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // ── PATCH /api/users/{userId}/status ─────────────────────
        public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            var previousStatus = user.Status;
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            // ── Lock / Unlock account ─────────────────────────────
            if (dto.Status == "Locked")
                await _userManager.SetLockoutEndDateAsync(
                    user, DateTimeOffset.UtcNow.AddYears(100));
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user,
                $"StatusChanged: {previousStatus} → {dto.Status}",
                "UserManagement");
        }

        // ── DELETE /api/users/{userId} ────────────────────────────
        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User already deleted.");

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate all tokens on delete
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "UserDeleted", "UserManagement");
        }

        // ── GET /api/users/{userId}/roles ─────────────────────────
        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            // Returns from Identity UserRoles table
            return await _userManager.GetRolesAsync(user);
        }

        // ── PUT /api/users/{userId}/roles — Admin only ────────────
        public async Task UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            // ── Validate all roles exist before assigning ─────────
            foreach (var role in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new InvalidOperationException(
                        $"Role '{role}' does not exist.");
            }

            var previousRoles = await _userManager.GetRolesAsync(user);

            // ── Assign roles and sync display field ───────────────
            await AssignRolesInternalAsync(user, dto.Roles.ToList());

            // ── Invalidate existing tokens — new roles take effect ─
            await _userManager.UpdateSecurityStampAsync(user);

            await LogAuditAsync(user,
                $"RoleUpdated: [{string.Join(", ", previousRoles)}] → [{string.Join(", ", dto.Roles)}]",
                "UserManagement");
        }

        // ── Private: Assign Roles Internal ───────────────────────
        // Shared by CreateUserAsync and UpdateUserRolesAsync
        private async Task AssignRolesInternalAsync(
            ApplicationUser user, List<string> roles)
        {
            // Remove all existing roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, existingRoles);

            // Assign new roles in Identity UserRoles table
            var result = await _userManager.AddToRolesAsync(user, roles);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // Sync user.Role display field
            user.Role = string.Join(", ", roles);
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        // ── Private: Map to DTO ───────────────────────────────────
        private static UserDto MapToDto(ApplicationUser user, IList<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = string.Join(", ", roles),
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // ── Private: Audit Logger ─────────────────────────────────
        private async Task LogAuditAsync(
            ApplicationUser user, string action, string resource)
        {
            var audit = new AuditLogs
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Action = action,
                Resource = resource,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(audit);
            await _context.SaveChangesAsync();
        }
    }
}