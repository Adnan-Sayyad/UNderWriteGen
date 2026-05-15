using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyBindingIssuanceAndEndorsements.DTOs;
using PolicyBindingIssuanceAndEndorsements.Services;

namespace PolicyBindingIssuanceAndEndorsements.Controllers
{
    [ApiController]
    [Route("api/policies")]
    [Authorize(Roles = "Operations,Compliance,Admin")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _service;

        public PoliciesController(IPolicyService service) => _service = service;

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{policyId:guid}")]
        public IActionResult GetById(Guid policyId)
        {
            var policy = _service.GetById(policyId);
            return policy is null ? NotFound(new { message = $"Policy {policyId} not found." }) : Ok(policy);
        }

        [HttpGet("number/{policyNumber}")]
        public IActionResult GetByPolicyNumber(string policyNumber)
        {
            var policy = _service.GetByPolicyNumber(policyNumber);
            return policy is null ? NotFound(new { message = $"Policy number '{policyNumber}' not found." }) : Ok(policy);
        }

        [HttpGet("submission/{submissionId:guid}")]
        public IActionResult GetBySubmission(Guid submissionId)
        {
            var policy = _service.GetBySubmission(submissionId);
            return policy is null ? NotFound(new { message = $"No policy found for submission {submissionId}." }) : Ok(policy);
        }

        [HttpGet("product-line/{line}")]
        public IActionResult GetByProductLine(string line) =>
            Ok(_service.GetByProductLine(line));

        [HttpGet("expiring")]
        public IActionResult GetExpiring([FromQuery] int daysAhead = 30) =>
            Ok(_service.GetExpiring(daysAhead));

        [HttpGet("{policyId:guid}/document")]
        public IActionResult GetDocument(Guid policyId)
        {
            var policy = _service.GetById(policyId);
            if (policy is null) return NotFound(new { message = $"Policy {policyId} not found." });
            return Ok(new { policyId, document = $"PolicyDocument_{policy.PolicyNumber}.pdf", generatedAt = DateTime.UtcNow });
        }

        [HttpGet("{policyId:guid}/certificate")]
        public IActionResult GetCertificate(Guid policyId)
        {
            var policy = _service.GetById(policyId);
            if (policy is null) return NotFound(new { message = $"Policy {policyId} not found." });
            return Ok(new { policyId, certificate = $"CertificateOfInsurance_{policy.PolicyNumber}.pdf", generatedAt = DateTime.UtcNow });
        }

        [HttpPost]
        [Authorize(Roles = "Operations,Admin")]
        public IActionResult Bind([FromBody] CreatePolicyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PolicyNumber))
                return BadRequest(new { message = "PolicyNumber is required." });
            if (string.IsNullOrWhiteSpace(dto.ProductLine))
                return BadRequest(new { message = "ProductLine is required." });

            var existing = _service.GetByPolicyNumber(dto.PolicyNumber);
            if (existing is not null)
                return Conflict(new { message = $"PolicyNumber '{dto.PolicyNumber}' already exists." });

            var policy = _service.Bind(dto);
            return CreatedAtAction(nameof(GetById), new { policyId = policy.PolicyID }, policy);
        }

        [HttpPut("{policyId:guid}")]
        public IActionResult Update(Guid policyId, [FromBody] UpdatePolicyDto dto)
        {
            var updated = _service.Update(policyId, dto);
            return updated is null ? NotFound(new { message = $"Policy {policyId} not found." }) : Ok(updated);
        }

        [HttpPatch("{policyId:guid}/status")]
        public IActionResult UpdateStatus(Guid policyId, [FromBody] UpdatePolicyStatusDto dto)
        {
            var validStatuses = new[] { "Active", "Cancelled", "Expired" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid status. Valid: {string.Join(", ", validStatuses)}" });

            var updated = _service.UpdateStatus(policyId, dto);
            return updated is null ? NotFound(new { message = $"Policy {policyId} not found." }) : Ok(updated);
        }

        [HttpDelete("{policyId:guid}")]
        public IActionResult Delete(Guid policyId)
        {
            if (!_service.Delete(policyId))
                return NotFound(new { message = $"Policy {policyId} not found." });
            return NoContent();
        }
    }
}
