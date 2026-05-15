using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        // ── POST /api/auth/register ───────────────────────────────
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var user = await _authService.RegisterAsync(dto);
                return StatusCode(201, ApiResponseDto<UserDto>.Ok(
                    $"User registered successfully. Admin can assign a role via POST /api/auth/assign-role using userId '{user.Id}'.",
                    user));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── POST /api/auth/assign-role ────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var user = await _authService.AssignRoleAsync(dto);
                return Ok(ApiResponseDto<UserDto>.Ok(
                    $"Role(s) '{string.Join(", ", dto.Roles)}' assigned successfully to user '{user.Email}'.",
                    user));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── POST /api/auth/login ──────────────────────────────────
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(ApiResponseDto<AuthResponseDto>.Ok("Login successful.", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── POST /api/auth/logout ─────────────────────────────────
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                await _authService.LogoutAsync(dto);
                return Ok(ApiResponseDto.Ok("Logged out successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── POST /api/auth/refresh-token ──────────────────────────
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var result = await _authService.RefreshTokenAsync(dto);
                return Ok(ApiResponseDto<AuthResponseDto>.Ok("Token refreshed successfully.", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(401, new
                {
                    Success = false,
                    Message = "Invalid data provided. Please provide a valid token."
                });
            }
        }

        // ── POST /api/auth/change-password ────────────────────────
        // Requires Bearer token via Swagger Authorize — userId extracted from JWT.
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors  = ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value!.Errors.Select(e => e.ErrorMessage))
                });

            // ── Extract userId from the validated JWT ──────────────
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !Guid.TryParse(userIdClaim, out var userId))
                return StatusCode(401, new
                {
                    Success = false,
                    Message = "Invalid token. Please log in again."
                });

            try
            {
                await _authService.ChangePasswordAsync(userId, dto);
                return Ok(ApiResponseDto.Ok(
                    "Password changed successfully. Please log in again with your new password."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

    }
}
