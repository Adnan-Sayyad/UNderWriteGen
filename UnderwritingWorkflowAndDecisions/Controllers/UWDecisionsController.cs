using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnderwritingWorkflowAndDecisions.DTOs;
using UnderwritingWorkflowAndDecisions.Services;

namespace UnderwritingWorkflowAndDecisions.Controllers
{
    [ApiController]
    [Route("api/uw-decisions")]
    [Authorize(Roles = "Underwriter,UWAssistant,Admin")]
    public class UWDecisionsController : ControllerBase
    {
        private readonly IUWDecisionService _service;

        public UWDecisionsController(IUWDecisionService service) => _service = service;

        [HttpGet("{submissionId:guid}")]
        public IActionResult GetBySubmission(Guid submissionId) =>
            Ok(_service.GetBySubmission(submissionId));

        [HttpGet("detail/{decisionId:guid}")]
        public IActionResult GetById(Guid decisionId)
        {
            var decision = _service.GetById(decisionId);
            return decision is null ? NotFound(new { message = $"Decision {decisionId} not found." }) : Ok(decision);
        }

        [HttpGet("decided-by/{userId:guid}")]
        public IActionResult GetByDecidedUser(Guid userId) =>
            Ok(_service.GetByDecidedUser(userId));

        [HttpGet("type/{decision}")]
        public IActionResult GetByType(string decision)
        {
            var validTypes = new[] { "Approve", "Decline", "Refer", "MoreInfo" };
            if (!validTypes.Contains(decision, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid decision type. Valid: {string.Join(", ", validTypes)}" });
            return Ok(_service.GetByType(decision));
        }

        [HttpPost]
        [Authorize(Roles = "Underwriter,Admin")]
        public IActionResult Add([FromBody] CreateUWDecisionDto dto)
        {
            var validTypes = new[] { "Approve", "Decline", "Refer", "MoreInfo" };
            if (!validTypes.Contains(dto.Decision, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid decision. Valid: {string.Join(", ", validTypes)}" });

            var decision = _service.Add(dto);
            return CreatedAtAction(nameof(GetById), new { decisionId = decision.DecisionID }, decision);
        }

        [HttpPut("{decisionId:guid}")]
        [Authorize(Roles = "Underwriter,Admin")]
        public IActionResult Update(Guid decisionId, [FromBody] UpdateUWDecisionDto dto)
        {
            var validTypes = new[] { "Approve", "Decline", "Refer", "MoreInfo" };
            if (!validTypes.Contains(dto.Decision, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Invalid decision. Valid: {string.Join(", ", validTypes)}" });

            var updated = _service.Update(decisionId, dto);
            return updated is null ? NotFound(new { message = $"Decision {decisionId} not found." }) : Ok(updated);
        }
    }
}
