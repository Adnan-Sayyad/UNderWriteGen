using IdentityAndAccessManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAndAccessManagement.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Roles = "Admin")]  // Only Admin can access audit logs
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // ── GET /api/audit-logs?page=1&pageSize=10 ────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _auditLogService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        // ── GET /api/audit-logs/{auditId} ─────────────────────────
        [HttpGet("{auditId:guid}")]
        public async Task<IActionResult> GetById(Guid auditId)
        {
            var result = await _auditLogService.GetByIdAsync(auditId);
            return Ok(result);
        }

        // ── GET /api/audit-logs/user/{userId} ─────────────────────
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _auditLogService.GetByUserIdAsync(userId);
            return Ok(result);
        }

        // ── GET /api/audit-logs/resource/{resource} ───────────────
        [HttpGet("resource/{resource}")]
        public async Task<IActionResult> GetByResource(string resource)
        {
            var result = await _auditLogService.GetByResourceAsync(resource);
            return Ok(result);
        }
    }
}