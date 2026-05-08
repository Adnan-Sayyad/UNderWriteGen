using IdentityAndAccessManagement.Data;
using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Models;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityAndAccessManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationUserDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            ApplicationUserDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        // ── Register ──────────────────────────────────────────────
        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {
            // ── Step 1: Check duplicate email ─────────────────────
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email is already in use.");

            // ── Step 2: Build user object (no role assigned yet) ──
            var user = new ApplicationUser
            {
                Id             = Guid.NewGuid(),
                FirstName      = dto.FirstName,
                LastName       = dto.LastName,
                Email          = dto.Email,
                PhoneNumber    = dto.PhoneNumber,
                Role           = string.Empty,
                Status         = "Active",
                EmailConfirmed = true,
                CreatedAt      = DateTime.UtcNow
            };

            // ── Step 3: Save user ─────────────────────────────────
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // ── Step 4: Audit log ─────────────────────────────────
            await LogAuditAsync(user,
                "UserRegistered — awaiting role assignment by Admin",
                "Auth",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}");

            var roles = await _userManager.GetRolesAsync(user);
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
                CreatedAt   = user.CreatedAt,
                UpdatedAt   = user.UpdatedAt
            };
        }

        // ── Assign Role (Admin only) ──────────────────────────────
        public async Task<UserDto> AssignRoleAsync(AssignRoleDto dto)
        {
            // ── Step 1: Verify AdminId exists and is an Admin ─────
            var admin = await _userManager.FindByIdAsync(dto.AdminId.ToString())
                ?? throw new UnauthorizedAccessException("Admin not found.");

            if (admin.IsDeleted)
                throw new UnauthorizedAccessException("Admin not found.");

            var adminRoles = await _userManager.GetRolesAsync(admin);
            if (!adminRoles.Contains("Admin"))
                throw new UnauthorizedAccessException(
                    "Only an Admin is authorized to assign roles.");

            // ── Step 2: Find target user ──────────────────────────
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            // ── Step 3: Validate all roles exist in Roles table ───
            foreach (var role in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new InvalidOperationException(
                        $"Role '{role}' does not exist.");
            }

            // ── Step 4: Clear existing UserRoles entries ──────────
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                if (!removeResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            }

            // ── Step 5: Write new entries into UserRoles table ────
            var addResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!addResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", addResult.Errors.Select(e => e.Description)));

            // ── Step 6: Sync Role display field in Users table ────
            user.Role               = string.Join(", ", dto.Roles);
            user.RefreshToken       = null; // force re-login so new role is in JWT
            user.RefreshTokenExpiry = null;
            user.UpdatedAt          = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ── Step 7: Audit log ─────────────────────────────────
            await LogAuditAsync(admin,
                $"Admin '{admin.Email}' assigned roles [{string.Join(", ", dto.Roles)}] to user '{user.Email}'",
                "Auth",
                $"TargetUser: {user.FirstName} {user.LastName} ({user.Email}), AssignedRoles: [{string.Join(", ", dto.Roles)}]");

            // ── Step 8: Read back from UserRoles table to confirm ─
            var confirmedRoles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id          = user.Id,
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                UserName    = user.UserName ?? string.Empty,
                Email       = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role        = string.Join(", ", confirmedRoles),
                Status      = user.Status,
                CreatedAt   = user.CreatedAt,
                UpdatedAt   = user.UpdatedAt
            };
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // ── Step 1: Validate user ─────────────────────────────
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException(
                    $"No account found with email '{dto.Email}'. Please check your email or register first.");

            if (user.IsDeleted)
                throw new UnauthorizedAccessException(
                    "This account has been permanently deactivated. Please contact the administrator.");

            if (user.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "Your account is locked. Please contact the administrator to unlock it.");

            if (user.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "Your account has been disabled. Please contact the administrator.");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException(
                    "Incorrect password. Please try again.");

            // ── Step 2: Get roles from UserRoles table ────────────
            var roles = await _userManager.GetRolesAsync(user);

            // ── Step 3: Generate both tokens ─────────────────────
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                                         GetExpiryMinutes());
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                                         GetRefreshTokenExpiryDays());

            // ── Step 4: Store refresh token + expiry in DB ────────
            user.RefreshToken       = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            user.UpdatedAt          = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ── Step 5: Audit log ─────────────────────────────────
            await LogAuditAsync(user, "Login", "Auth",
                $"Name: {user.FirstName} {user.LastName}, Role: {string.Join(", ", roles)}, Status: {user.Status}");

            return new AuthResponseDto
            {
                UserId             = user.Id,
                Email              = user.Email ?? string.Empty,
                Role               = string.Join(", ", roles),
                AccessToken        = accessToken,
                RefreshToken       = refreshToken,
                AccessTokenExpiry  = accessTokenExpiry,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        // ── Logout ────────────────────────────────────────────────
        public async Task LogoutAsync(LogoutDto dto)
        {
            // ── Step 1: Validate user exists ──────────────────────
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
                ?? throw new KeyNotFoundException(
                    $"No account found with ID '{dto.UserId}'. Please provide a valid user ID.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    "This account has been deactivated. Please contact the administrator.");

            if (user.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "This account is locked. Please contact the administrator.");

            if (user.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "This account has been disabled. Please contact the administrator.");

            // ── Step 2: Check if already logged out ───────────────
            if (user.RefreshToken == null)
                throw new InvalidOperationException(
                    "User is already logged out.");

            // ── Step 3: Clear refresh token + expiry ─────────────
            user.RefreshToken       = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt          = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "Logout", "Auth",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}");
        }

        // ── Refresh Token ─────────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            const string invalidMsg =
                "Invalid data provided. Please provide a valid token.";

            // ── Step 1: Parse claims from the (possibly expired) access token ──
            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            }
            catch
            {
                throw new UnauthorizedAccessException(invalidMsg);
            }

            // ── Step 2: Extract userId claim ──────────────────────
            // ASP.NET's JWT handler remaps "sub" → ClaimTypes.NameIdentifier,
            // so we must check both to reliably get the userId.
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException(invalidMsg);

            // ── Step 3: Locate user in DB ─────────────────────────
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new UnauthorizedAccessException(invalidMsg);

            // ── Step 4: Account status checks ─────────────────────
            if (user.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "Your account is locked. Please contact the administrator.");

            if (user.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "Your account has been disabled. Please contact the administrator.");

            // ── Step 5: Refresh token must exist in DB ────────────
            if (string.IsNullOrWhiteSpace(user.RefreshToken))
                throw new UnauthorizedAccessException(invalidMsg);

            // ── Step 6: Refresh token must match exactly ──────────
            if (user.RefreshToken != dto.RefreshToken)
                throw new UnauthorizedAccessException(invalidMsg);

            // ── Step 7: Refresh token must not be expired ─────────
            if (user.RefreshTokenExpiry == null ||
                user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new UnauthorizedAccessException(invalidMsg);

            // ── Step 8: Get latest roles from DB ──────────────────
            var roles = await _userManager.GetRolesAsync(user);

            // ── Step 9: Rotate — generate a brand-new token pair ──
            var newAccessToken    = GenerateAccessToken(user, roles);
            var newRefreshToken   = GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(GetExpiryMinutes());
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

            // ── Step 10: Persist new tokens + expiry to DB ────────
            user.RefreshToken       = newRefreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            user.UpdatedAt          = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ── Step 11: Audit log ────────────────────────────────
            await LogAuditAsync(user, "RefreshToken", "Auth",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}");

            return new AuthResponseDto
            {
                UserId             = user.Id,
                Email              = user.Email ?? string.Empty,
                Role               = string.Join(", ", roles),
                AccessToken        = newAccessToken,
                RefreshToken       = newRefreshToken,
                AccessTokenExpiry  = accessTokenExpiry,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        // ── Change Password ───────────────────────────────────────
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            // ── Step 1: Locate user from JWT claim ────────────────
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException(
                    "User account not found. Please log in again.");

            if (user.IsDeleted)
                throw new KeyNotFoundException(
                    "This account has been deactivated. Please contact the administrator.");

            // ── Step 2: Account status checks ─────────────────────
            if (user.Status == "Locked")
                throw new UnauthorizedAccessException(
                    "Your account is locked. Please contact the administrator.");

            if (user.Status == "Disabled")
                throw new UnauthorizedAccessException(
                    "Your account has been disabled. Please contact the administrator.");

            // ── Step 3: Active session check ──────────────────────
            if (string.IsNullOrWhiteSpace(user.RefreshToken))
                throw new UnauthorizedAccessException(
                    "No active session found. Please log in first.");

            // ── Step 4: Verify current password is correct ─────────
            if (!await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
                throw new InvalidOperationException(
                    "Current password is incorrect. Please try again.");

            // ── Step 5: New password must differ from current ──────
            if (dto.CurrentPassword == dto.NewPassword)
                throw new InvalidOperationException(
                    "New password must be different from your current password.");

            // ── Step 6: Apply password change (Identity enforces rules) ──
            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(" ", result.Errors.Select(e => e.Description)));

            // ── Step 7: Invalidate session — force re-login ────────
            user.RefreshToken       = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt          = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ── Step 8: Audit log ─────────────────────────────────
            await LogAuditAsync(user, "ChangePassword", "Auth",
                $"Name: {user.FirstName} {user.LastName}, Email: {user.Email}");
        }

        // ── Private: Generate Access Token ────────────────────────
        private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Name,  user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            };

            // Add each role as separate claim
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetExpiryMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── Private: Generate Refresh Token ───────────────────────
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        // ── Private: Validate a LIVE (non-expired) Access Token ──────
        // Used by ChangePassword — rejects expired tokens.
        private ClaimsPrincipal GetPrincipalFromValidToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,   // ← enforce expiry
                ValidateIssuerSigningKey = true,
                ValidIssuer              = _configuration["Jwt:Issuer"],
                ValidAudience            = _configuration["Jwt:Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(
                                                   _configuration["Jwt:SecretKey"]!)),
                ClockSkew                = TimeSpan.Zero
            };

            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }

        // ── Private: Validate Expired Access Token ────────────────
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, 
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(
                                                   _configuration["Jwt:SecretKey"]!))
            };

            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }

        // ── Private: Get Expiry Minutes ───────────────────────────
        private double GetExpiryMinutes()
        {
            return Convert.ToDouble(
                _configuration["Jwt:ExpiryMinutes"] ?? "60");
        }

        // ── Private: Get Refresh Token Expiry Days ────────────────
        private double GetRefreshTokenExpiryDays()
        {
            return Convert.ToDouble(
                _configuration["Jwt:RefreshTokenExpiryDays"] ?? "30");
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