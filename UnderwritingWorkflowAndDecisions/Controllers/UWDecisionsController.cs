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
        private readonly IAiSummaryService  _aiSummary;

        public UWDecisionsController(IUWDecisionService service, IAiSummaryService aiSummary)
        {
            _service   = service;
            _aiSummary = aiSummary;
        }

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

        /// <summary>
        /// GET /api/uw-decisions/submissions/{submissionId}/ai-summary
        /// Returns a Claude-generated plain-English risk summary for the underwriter,
        /// built from real submission data and UW decision/note history.
        /// </summary>
        [HttpGet("submissions/{submissionId:guid}/ai-summary")]
        public async Task<IActionResult> GetAiSummary(Guid submissionId, CancellationToken ct)
        {
            try
            {
                var result = await _aiSummary.SummarizeSubmissionAsync(submissionId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "AI summary failed.", detail = ex.Message });
            }
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
