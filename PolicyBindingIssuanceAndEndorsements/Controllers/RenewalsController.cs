using Microsoft.AspNetCore.Mvc;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Services;

namespace PolicyBindingIssuanceAndEndorsements.Controllers
{
    [ApiController]
    [Route("api/renewals")]
    public class RenewalsController : ControllerBase
    {
        private readonly IRenewalService _service;

        public RenewalsController(IRenewalService service) => _service = service;

        // GET /api/renewals/{policyId}
        [HttpGet("{policyId:guid}")]
        public IActionResult GetByPolicy(Guid policyId)
        {
            var item = _service.GetByPolicy(policyId);
            if (item is null) return NotFound(new { message = $"No renewal found for policy {policyId}." });
            return Ok(item);
        }

        // GET /api/renewals/detail/{renewalId}
        [HttpGet("detail/{renewalId:guid}")]
        public IActionResult GetById(Guid renewalId)
        {
            var item = _service.GetById(renewalId);
            if (item is null) return NotFound(new { message = $"Renewal {renewalId} not found." });
            return Ok(item);
        }

        // GET /api/renewals/pending
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            var items = _service.GetPending();
            return Ok(items);
        }

        // POST /api/renewals
        [HttpPost]
        public IActionResult Create([FromBody] CreateRenewalDto dto)
        {
            var item = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { renewalId = item.RenewalID }, item);
        }

        // PATCH /api/renewals/{renewalId}/status
        [HttpPatch("{renewalId:guid}/status")]
        public IActionResult UpdateStatus(Guid renewalId, [FromBody] UpdateRenewalStatusDto dto)
        {
            var validStatuses = new[] { "Offered", "Accepted", "Declined" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid status. Valid values: {string.Join(", ", validStatuses)}" });

            var updated = _service.UpdateStatus(renewalId, dto);
            if (updated is null) return NotFound(new { message = $"Renewal {renewalId} not found." });
            return Ok(updated);
        }
    }
}
