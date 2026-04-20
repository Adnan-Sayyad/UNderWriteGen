using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferralMatrixController : ControllerBase
    {
        private readonly IReferralMatrixService _service;

        public ReferralMatrixController(IReferralMatrixService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var matrices = await _service.GetAllMatricesAsync();
            return Ok(matrices);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var matrix = await _service.GetMatrixByIdAsync(id);
            if (matrix is null) return NotFound();
            return Ok(matrix);
        }

        [HttpGet("product/{productLine}")]
        public async Task<IActionResult> GetByProductLine(string productLine)
        {
            var matrices = await _service.GetMatricesByProductLineAsync(productLine);
            return Ok(matrices);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMatrices()
        {
            var matrices = await _service.GetActiveMatricesAsync();
            return Ok(matrices);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReferralMatrixDto dto)
        {
            var created = await _service.CreateMatrixAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ReferralMatrixID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReferralMatrixDto dto)
        {
            var updated = await _service.UpdateMatrixAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteMatrixAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
