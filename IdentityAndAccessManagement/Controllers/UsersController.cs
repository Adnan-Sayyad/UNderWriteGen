using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAndAccessManagement.Controllers
{
    [ApiController]
    [Route("api/users")]
    [AllowAnonymous]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ── GET /api/users?adminId=... ────────────────────────────
        // Accessible by: Admin, Manager
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your user ID as ?adminId=<your-guid>." });

            try
            {
                var users = await _userService.GetAllUsersAsync(adminId);
                return Ok(ApiResponseDto<IEnumerable<UserDto>>.Ok(
                    "Users retrieved successfully.", users));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── GET /api/users/{userId}?adminId=... ───────────────────
        // Accessible by: Admin, Manager
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId, [FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your user ID as ?adminId=<your-guid>." });

            try
            {
                var user = await _userService.GetUserByIdAsync(adminId, userId);
                return Ok(ApiResponseDto<UserDto>.Ok(
                    "User retrieved successfully.", user));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ── PUT /api/users/{userId}?adminId=... ───────────────────
        // Accessible by: Admin only
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromQuery] Guid adminId, [FromBody] UpdateUserDto dto)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your user ID as ?adminId=<your-guid>." });

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
                var user = await _userService.UpdateUserAsync(adminId, userId, dto);
                return Ok(ApiResponseDto<UserDto>.Ok("User updated successfully.", user));
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

        // ── PATCH /api/users/{userId}/status?adminId=... ──────────
        // Accessible by: Admin only
        [HttpPatch("{userId:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromQuery] Guid adminId, [FromBody] UpdateUserStatusDto dto)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your user ID as ?adminId=<your-guid>." });

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
                await _userService.UpdateUserStatusAsync(adminId, userId, dto);
                return Ok(ApiResponseDto.Ok(
                    $"User status updated to '{dto.Status}' successfully."));
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

        // ── DELETE /api/users/{userId}?adminId=... ────────────────
        // Accessible by: Admin only
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId, [FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your user ID as ?adminId=<your-guid>." });

            try
            {
                await _userService.DeleteUserAsync(adminId, userId);
                return Ok(ApiResponseDto.Ok("User deleted successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}
