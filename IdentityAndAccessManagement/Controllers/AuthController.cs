using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityAndAccessManagement.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ── POST /api/auth/login ──────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        // ── POST /api/auth/logout ─────────────────────────────────
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Extract userId from JWT claims
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("Invalid token.");

            await _authService.LogoutAsync(userId);

            return Ok(new { Message = "Logged out successfully." });
        }

        // ── POST /api/auth/refresh-token ──────────────────────────
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(result);
        }

        // ── POST /api/auth/change-password ────────────────────────
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract userId from JWT claims
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("Invalid token.");

            await _authService.ChangePasswordAsync(userId, dto);

            return Ok(new
            {
                Message = "Password changed successfully. Please login again."
            });
        }

        // ── POST /api/auth/forgot-password ────────────────────────
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authService.ForgotPasswordAsync(dto);

            // Always return same message — prevent email enumeration
            return Ok(new
            {
                Message = "If the email exists a reset link has been sent."
            });
        }

        // ── POST /api/auth/reset-password ─────────────────────────
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authService.ResetPasswordAsync(dto);

            return Ok(new
            {
                Message = "Password reset successfully. Please login again."
            });
        }
    }
}