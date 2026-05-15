using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using static RulesScoringAndReferralMatrix.DTOs.PaginationHelpers;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/referral-matrix")]
    [Authorize(Roles = "Underwriter,UWAssistant,Operations,Admin")]
    public class ReferralMatrixController : ControllerBase
    {
        private readonly IReferralMatrixService _service;

        public ReferralMatrixController(IReferralMatrixService service)
        {
            _service = service;
        }

        // GET /api/referral-matrix?page=0&size=20&productLine=Life&authority=UW1&status=Active
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? size,
            [FromQuery] string? productLine,
            [FromQuery] RequiredAuthority? authority,
            [FromQuery] UWStatus? status)
        {
            var (p, s) = Normalize(page, size);
            var result = await _service.GetMatricesPagedAsync(p, s, productLine, authority, status);
            return Ok(result);
        }

        // GET /api/referral-matrix/{matrixId}
        [HttpGet("{matrixId:guid}")]
        public async Task<IActionResult> GetById(Guid matrixId)
        {
            var matrix = await _service.GetMatrixByIdAsync(matrixId);
            if (matrix is null) return NotFound();
            return Ok(matrix);
        }

        // POST /api/referral-matrix
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReferralMatrixDto dto)
        {
            var created = await _service.CreateMatrixAsync(dto);
            return CreatedAtAction(nameof(GetById), new { matrixId = created.ReferralMatrixID }, created);
        }

        // PUT /api/referral-matrix/{matrixId}
        [HttpPut("{matrixId:guid}")]
        public async Task<IActionResult> Update(Guid matrixId, [FromBody] UpdateReferralMatrixDto dto)
        {
            var updated = await _service.UpdateMatrixAsync(matrixId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/referral-matrix/{matrixId}/status
        [HttpPatch("{matrixId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid matrixId, [FromBody] UpdateMatrixStatusDto dto)
        {
            var updated = await _service.UpdateMatrixStatusAsync(matrixId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/referral-matrix/{matrixId}
        [HttpDelete("{matrixId:guid}")]
        public async Task<IActionResult> Delete(Guid matrixId)
        {
            var deleted = await _service.DeleteMatrixAsync(matrixId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
