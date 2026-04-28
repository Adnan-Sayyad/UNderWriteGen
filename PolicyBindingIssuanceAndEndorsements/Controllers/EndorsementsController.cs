using Microsoft.AspNetCore.Mvc;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Services;

namespace PolicyBindingIssuanceAndEndorsements.Controllers
{
    [ApiController]
    [Route("api/endorsements")]
    public class EndorsementsController : ControllerBase
    {
        private readonly IEndorsementService _service;

        public EndorsementsController(IEndorsementService service) => _service = service;

        // GET /api/endorsements/{policyId}
        [HttpGet("{policyId:guid}")]
        public IActionResult GetByPolicy(Guid policyId)
        {
            var items = _service.GetByPolicy(policyId);
            return Ok(items);
        }

        // GET /api/endorsements/detail/{endorsementId}
        [HttpGet("detail/{endorsementId:guid}")]
        public IActionResult GetById(Guid endorsementId)
        {
            var item = _service.GetById(endorsementId);
            if (item is null) return NotFound(new { message = $"Endorsement {endorsementId} not found." });
            return Ok(item);
        }

        // GET /api/endorsements/type/{endorsementType}
        [HttpGet("type/{endorsementType}")]
        public IActionResult GetByType(string endorsementType)
        {
            var validTypes = new[] { "MidTermChange", "Address", "Limit", "Deductible", "Beneficiary" };
            if (!validTypes.Contains(endorsementType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid type. Valid values: {string.Join(", ", validTypes)}" });

            var items = _service.GetByType(endorsementType);
            return Ok(items);
        }

        // POST /api/endorsements
        [HttpPost]
        public IActionResult Add([FromBody] CreateEndorsementDto dto)
        {
            var validTypes = new[] { "MidTermChange", "Address", "Limit", "Deductible", "Beneficiary" };
            if (!validTypes.Contains(dto.EndorsementType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid EndorsementType. Valid values: {string.Join(", ", validTypes)}" });

            var item = _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { endorsementId = item.EndorsementID }, item);
        }

        // PUT /api/endorsements/{endorsementId}
        [HttpPut("{endorsementId:guid}")]
        public IActionResult Update(Guid endorsementId, [FromBody] UpdateEndorsementDto dto)
        {
            var updated = _service.Update(endorsementId, dto);
            if (updated is null) return NotFound(new { message = $"Endorsement {endorsementId} not found." });
            return Ok(updated);
        }

        // PATCH /api/endorsements/{endorsementId}/status
        [HttpPatch("{endorsementId:guid}/status")]
        public IActionResult UpdateStatus(Guid endorsementId, [FromBody] UpdateEndorsementStatusDto dto)
        {
            var validStatuses = new[] { "Proposed", "Approved", "Posted" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid status. Valid values: {string.Join(", ", validStatuses)}" });

            var updated = _service.UpdateStatus(endorsementId, dto);
            if (updated is null) return NotFound(new { message = $"Endorsement {endorsementId} not found." });
            return Ok(updated);
        }

        // DELETE /api/endorsements/{endorsementId}
        [HttpDelete("{endorsementId:guid}")]
        public IActionResult Delete(Guid endorsementId)
        {
            if (!_service.Delete(endorsementId))
                return NotFound(new { message = $"Endorsement {endorsementId} not found." });
            return NoContent();
        }
    }
}
