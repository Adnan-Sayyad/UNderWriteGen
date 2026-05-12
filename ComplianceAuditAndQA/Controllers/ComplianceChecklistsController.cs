using ComplianceAuditAndQA.DTOs;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAuditAndQA.Controllers
{
    [ApiController]
    [Route("api/compliance-checklists")]
    public class ComplianceChecklistsController : ControllerBase
    {
        private readonly IComplianceChecklistService _service;

        public ComplianceChecklistsController(IComplianceChecklistService service)
            => _service = service;

        // ── GET /api/compliance-checklists ────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<ComplianceChecklistDto>>
                .Ok("Checklists retrieved successfully.", data));
        }

        // ── GET /api/compliance-checklists/{checklistId} ──────────
        [HttpGet("{checklistId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid checklistId)
        {
            var data = await _service.GetByIdAsync(checklistId);
            return Ok(ApiResponseDto<ComplianceChecklistDto>
                .Ok("Checklist retrieved successfully.", data));
        }

        // ── GET /api/compliance-checklists/submission/{submissionId}
        [HttpGet("submission/{submissionId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySubmissionId(Guid submissionId)
        {
            var data = await _service.GetBySubmissionIdAsync(submissionId);
            return Ok(ApiResponseDto<ComplianceChecklistDto>
                .Ok("Checklist retrieved successfully.", data));
        }

        // ── POST /api/compliance-checklists ───────────────────────
        [HttpPost]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateComplianceChecklistDto dto)
        {
            var data = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { checklistId = data.ChecklistId },
                ApiResponseDto<ComplianceChecklistDto>
                    .Ok("Checklist created successfully.", data));
        }

        // ── PUT /api/compliance-checklists/{checklistId} ──────────
        [HttpPut("{checklistId:guid}")]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> Update(
            Guid checklistId, [FromBody] UpdateComplianceChecklistDto dto)
        {
            var data = await _service.UpdateAsync(checklistId, dto);
            return Ok(ApiResponseDto<ComplianceChecklistDto>
                .Ok("Checklist updated successfully.", data));
        }

        // ── PATCH /api/compliance-checklists/{checklistId}/status ─
        [HttpPatch("{checklistId:guid}/status")]
        [Authorize(Roles = "Compliance,Admin")]
        public async Task<IActionResult> UpdateStatus(
            Guid checklistId, [FromBody] UpdateChecklistStatusDto dto)
        {
            var data = await _service.UpdateStatusAsync(checklistId, dto);
            return Ok(ApiResponseDto<ComplianceChecklistDto>
                .Ok("Checklist status updated successfully.", data));
        }
    }
}
