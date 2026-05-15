using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Services;

namespace PolicyBindingIssuanceAndEndorsements.Controllers
{
    [ApiController]
    [Route("api/cancellations")]
    [Authorize(Roles = "Operations,Admin")]
    public class CancellationsController : ControllerBase
    {
        private readonly ICancellationService _service;

        public CancellationsController(ICancellationService service) => _service = service;

        // GET /api/cancellations/{policyId}
        [HttpGet("{policyId:guid}")]
        public IActionResult GetByPolicy(Guid policyId)
        {
            var item = _service.GetByPolicy(policyId);
            if (item is null) return NotFound(new { message = $"No cancellation found for policy {policyId}." });
            return Ok(item);
        }

        // GET /api/cancellations/detail/{cancellationId}
        [HttpGet("detail/{cancellationId:guid}")]
        public IActionResult GetById(Guid cancellationId)
        {
            var item = _service.GetById(cancellationId);
            if (item is null) return NotFound(new { message = $"Cancellation {cancellationId} not found." });
            return Ok(item);
        }

        // POST /api/cancellations
        [HttpPost]
        public new IActionResult Request([FromBody] CreateCancellationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CancelReason))
                return BadRequest(new { message = "CancelReason is required." });

            var item = _service.Request(dto);
            return CreatedAtAction(nameof(GetById), new { cancellationId = item.CancellationID }, item);
        }

        // PATCH /api/cancellations/{cancellationId}/status
        [HttpPatch("{cancellationId:guid}/status")]
        public IActionResult UpdateStatus(Guid cancellationId, [FromBody] UpdateCancellationStatusDto dto)
        {
            var validStatuses = new[] { "Requested", "Approved", "Posted" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid status. Valid values: {string.Join(", ", validStatuses)}" });

            var updated = _service.UpdateStatus(cancellationId, dto);
            if (updated is null) return NotFound(new { message = $"Cancellation {cancellationId} not found." });
            return Ok(updated);
        }
    }
}
