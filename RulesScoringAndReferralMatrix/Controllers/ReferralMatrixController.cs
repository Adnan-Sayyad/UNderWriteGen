using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/referral-matrix")]
    public class ReferralMatrixController : ControllerBase
    {
        private readonly IReferralMatrixService _service;

        public ReferralMatrixController(IReferralMatrixService service)
        {
            _service = service;
        }

        // GET /api/referral-matrix
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var matrices = await _service.GetAllMatricesAsync();
            return Ok(matrices);
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
