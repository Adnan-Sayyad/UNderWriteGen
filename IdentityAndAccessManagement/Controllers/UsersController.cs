using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAndAccessManagement.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ── GET /api/users ────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // ── GET /api/users/{userId} ───────────────────────────────
        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return Ok(user);
        }

        // ── POST /api/users ───────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(
                nameof(GetUserById),
                new { userId = user.Id },
                user);
        }

        // ── PUT /api/users/{userId} ───────────────────────────────
        [HttpPut("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(
            Guid userId, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(userId, dto);
            return Ok(user);
        }

        // ── PATCH /api/users/{userId}/status ─────────────────────
        [HttpPatch("{userId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(
            Guid userId, [FromBody] UpdateUserStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.UpdateUserStatusAsync(userId, dto);
            return Ok(new { Message = "User status updated successfully." });
        }

        // ── DELETE /api/users/{userId} ────────────────────────────
        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(new { Message = "User deleted successfully." });
        }

        // ── GET /api/users/{userId}/roles ─────────────────────────
        [HttpGet("{userId:guid}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        // ── PUT /api/users/{userId}/roles — Admin full control ────
        [HttpPut("{userId:guid}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRoles(
            Guid userId, [FromBody] UpdateUserRolesDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.UpdateUserRolesAsync(userId, dto);
            return Ok(new { Message = "User roles updated successfully." });
        }
    }
}