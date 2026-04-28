using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAuditAndQA.Controllers
{
    [ApiController]
    [Route("api/authority-breaches")]
    [Authorize]
    public class AuthorityBreachesController : ControllerBase
    {
        private readonly IAuthorityBreachService _service;

        public AuthorityBreachesController(IAuthorityBreachService service)
            => _service = service;

        // ── GET /api/authority-breaches ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<AuthorityBreachDto>>
                .Ok("Authority breaches retrieved successfully.", data));
        }

        // ── GET /api/authority-breaches/{breachId} ────────────────
        [HttpGet("{breachId:guid}")]
        public async Task<IActionResult> GetById(Guid breachId)
        {
            var data = await _service.GetByIdAsync(breachId);
            return Ok(ApiResponseDto<AuthorityBreachDto>
                .Ok("Authority breach retrieved successfully.", data));
        }

        // ── GET /api/authority-breaches/submission/{submissionId} ─
        [HttpGet("submission/{submissionId:guid}")]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var data = await _service.GetBySubmissionIdAsync(submissionId);
            return Ok(ApiResponseDto<IEnumerable<AuthorityBreachDto>>
                .Ok("Authority breaches retrieved successfully.", data));
        }

        // ── GET /api/authority-breaches/type/{breachType} ────────
        [HttpGet("type/{breachType}")]
        public async Task<IActionResult> GetByBreachType(string breachType)
        {
            var data = await _service.GetByBreachTypeAsync(breachType);
            return Ok(ApiResponseDto<IEnumerable<AuthorityBreachDto>>
                .Ok("Authority breaches retrieved successfully.", data));
        }

        // ── POST /api/authority-breaches ──────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuthorityBreachDto dto)
        {
            var data = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { breachId = data.BreachId },
                ApiResponseDto<AuthorityBreachDto>
                    .Ok("Authority breach created successfully.", data));
        }

        // ── PUT /api/authority-breaches/{breachId} ────────────────
        [HttpPut("{breachId:guid}")]
        public async Task<IActionResult> Update(
            Guid breachId, [FromBody] UpdateAuthorityBreachDto dto)
        {
            var data = await _service.UpdateAsync(breachId, dto);
            return Ok(ApiResponseDto<AuthorityBreachDto>
                .Ok("Authority breach updated successfully.", data));
        }

        // ── PATCH /api/authority-breaches/{breachId}/status ───────
        [HttpPatch("{breachId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid breachId, [FromBody] UpdateBreachStatusDto dto)
        {
            var data = await _service.UpdateStatusAsync(breachId, dto);
            return Ok(ApiResponseDto<AuthorityBreachDto>
                .Ok("Authority breach status updated successfully.", data));
        }
    }
}
