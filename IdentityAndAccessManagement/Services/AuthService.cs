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
        private readonly IConfiguration _configuration;
        private readonly ApplicationUserDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationUserDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // ── Step 1: Validate user ─────────────────────────────
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (user.IsDeleted)
                throw new UnauthorizedAccessException("Account has been deactivated.");

            if (user.Status == "Locked" || user.Status == "Disabled")
                throw new UnauthorizedAccessException($"Account is {user.Status.ToLower()}.");

            // ── Step 2: Get roles from UserRoles table ────────────
            var roles = await _userManager.GetRolesAsync(user);

            // ── Step 3: Generate both tokens ─────────────────────
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                                         GetExpiryMinutes());
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                                         GetRefreshTokenExpiryDays());

            // ── Step 4: Store refresh token + expiry ──────────────
            user.SecurityStamp = refreshToken;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // ── Step 5: Audit log ─────────────────────────────────
            await LogAuditAsync(user, "Login", "Auth");

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        // ── Logout ────────────────────────────────────────────────
        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // ── Invalidate refresh token ──────────────────────────
            // SecurityStamp regenerated — old refresh token useless
            await _userManager.UpdateSecurityStampAsync(user);

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "Logout", "Auth");
        }

        // ── Refresh Token ─────────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // ── Step 1: Extract claims from expired access token ──
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("Invalid token.");

            // ── Step 2: Validate user ─────────────────────────────
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (user.IsDeleted)
                throw new UnauthorizedAccessException("Account has been deactivated.");

            if (user.Status == "Locked" || user.Status == "Disabled")
                throw new UnauthorizedAccessException($"Account is {user.Status.ToLower()}.");

            // ── Step 3: Validate refresh token ────────────────────
            if (user.SecurityStamp != dto.RefreshToken)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            // ── Step 4: Get latest roles ──────────────────────────
            var roles = await _userManager.GetRolesAsync(user);

            // ── Step 5: Rotate — generate brand new token pair ────
            var newAccessToken = GenerateAccessToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                                            GetExpiryMinutes());
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                                            GetRefreshTokenExpiryDays());

            // ── Step 6: Invalidate old — store new refresh token ──
            user.SecurityStamp = newRefreshToken;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "RefreshToken", "Auth");

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        // ── Change Password ───────────────────────────────────────
        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // ── Invalidate refresh token — force re-login ─────────
            await _userManager.UpdateSecurityStampAsync(user);

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "ChangePassword", "Auth");
        }

        // ── Forgot Password ───────────────────────────────────────
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Always return silently — prevent email enumeration
            if (user == null || user.IsDeleted) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: Plug in your email service here
            // await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            Console.WriteLine($"[DEV] Reset token for {dto.Email}: {token}");

            await LogAuditAsync(user, "ForgotPassword", "Auth");
        }

        // ── Reset Password ────────────────────────────────────────
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.IsDeleted)
                throw new KeyNotFoundException("User not found.");

            var result = await _userManager.ResetPasswordAsync(
                user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            // ── Invalidate refresh token — force re-login ─────────
            await _userManager.UpdateSecurityStampAsync(user);

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync(user, "ResetPassword", "Auth");
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