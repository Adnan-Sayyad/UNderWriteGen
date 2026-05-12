using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAuditAndQA.Controllers
{
    [ApiController]
    [Route("api/exception-logs")]
    public class ExceptionLogsController : ControllerBase
    {
        private readonly IExceptionLogService _service;

        public ExceptionLogsController(IExceptionLogService service)
            => _service = service;

        // ── GET /api/exception-logs ───────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<ExceptionLogDto>>
                .Ok("Exception logs retrieved successfully.", data));
        }

        // ── GET /api/exception-logs/{exceptionId} ─────────────────
        [HttpGet("{exceptionId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid exceptionId)
        {
            var data = await _service.GetByIdAsync(exceptionId);
            return Ok(ApiResponseDto<ExceptionLogDto>
                .Ok("Exception log retrieved successfully.", data));
        }

        // ── GET /api/exception-logs/submission/{submissionId} ─────
        [HttpGet("submission/{submissionId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var data = await _service.GetBySubmissionIdAsync(submissionId);
            return Ok(ApiResponseDto<IEnumerable<ExceptionLogDto>>
                .Ok("Exception logs retrieved successfully.", data));
        }

        // ── GET /api/exception-logs/category/{category} ───────────
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var data = await _service.GetByCategoryAsync(category);
            return Ok(ApiResponseDto<IEnumerable<ExceptionLogDto>>
                .Ok("Exception logs retrieved successfully.", data));
        }

        // ── POST /api/exception-logs ──────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateExceptionLogDto dto)
        {
            var data = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { exceptionId = data.ExceptionId },
                ApiResponseDto<ExceptionLogDto>
                    .Ok("Exception log created successfully.", data));
        }

        // ── PUT /api/exception-logs/{exceptionId} ─────────────────
        [HttpPut("{exceptionId:guid}")]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> Update(
            Guid exceptionId, [FromBody] UpdateExceptionLogDto dto)
        {
            var data = await _service.UpdateAsync(exceptionId, dto);
            return Ok(ApiResponseDto<ExceptionLogDto>
                .Ok("Exception log updated successfully.", data));
        }

        // ── PATCH /api/exception-logs/{exceptionId}/status ────────
        [HttpPatch("{exceptionId:guid}/status")]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> UpdateStatus(
            Guid exceptionId, [FromBody] UpdateExceptionStatusDto dto)
        {
            var data = await _service.UpdateStatusAsync(exceptionId, dto);
            return Ok(ApiResponseDto<ExceptionLogDto>
                .Ok("Exception log status updated successfully.", data));
        }
    }
}
