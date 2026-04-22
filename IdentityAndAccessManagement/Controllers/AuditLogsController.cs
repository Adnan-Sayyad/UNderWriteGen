using IdentityAndAccessManagement.DTOs;
using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAndAccessManagement.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [AllowAnonymous]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // ── GET /api/audit-logs?adminId=... ───────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your admin user ID as ?adminId=<your-guid>." });

            try
            {
                var result = await _auditLogService.GetAllAsync(adminId);
                return Ok(ApiResponseDto<IEnumerable<AuditLogDto>>.Ok(
                    "Audit logs retrieved successfully.", result));
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

        // ── GET /api/audit-logs/{auditId}?adminId=... ────────────
        [HttpGet("{auditId:guid}")]
        public async Task<IActionResult> GetById(Guid auditId, [FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your admin user ID as ?adminId=<your-guid>." });

            try
            {
                var result = await _auditLogService.GetByIdAsync(adminId, auditId);
                return Ok(ApiResponseDto<AuditLogDto>.Ok(
                    "Audit log retrieved successfully.", result));
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

        // ── GET /api/audit-logs/user/{userId}?adminId=... ─────────
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId, [FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your admin user ID as ?adminId=<your-guid>." });

            try
            {
                var result = await _auditLogService.GetByUserIdAsync(adminId, userId);
                return Ok(ApiResponseDto<IEnumerable<AuditLogDto>>.Ok(
                    "Audit logs retrieved successfully.", result));
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

        // ── GET /api/audit-logs/resource/{resource}?adminId=... ──
        [HttpGet("resource/{resource}")]
        public async Task<IActionResult> GetByResource(string resource, [FromQuery] Guid adminId)
        {
            if (adminId == Guid.Empty)
                return BadRequest(new { Success = false, Message = "Admin ID is required. Pass your admin user ID as ?adminId=<your-guid>." });

            if (string.IsNullOrWhiteSpace(resource))
                return BadRequest(new { Success = false, Message = "Resource is required. Valid values: 'Auth' or 'UserManagement'." });

            try
            {
                var result = await _auditLogService.GetByResourceAsync(adminId, resource);
                return Ok(ApiResponseDto<IEnumerable<AuditLogDto>>.Ok(
                    "Audit logs retrieved successfully.", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
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
