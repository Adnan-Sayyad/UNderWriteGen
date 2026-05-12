using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.configs.Enums;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;
using static RulesScoringAndReferralMatrix.DTOs.PaginationHelpers;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/uw-rules")]
    public class UWRulesController : ControllerBase
    {
        private readonly IUWRuleService _service;

        public UWRulesController(IUWRuleService service)
        {
            _service = service;
        }

        // GET /api/uw-rules?page=0&size=20&productLine=Life&severity=Block&status=Active
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? size,
            [FromQuery] string? productLine,
            [FromQuery] Severity? severity,
            [FromQuery] UWStatus? status)
        {
            var (p, s) = Normalize(page, size);
            var result = await _service.GetRulesPagedAsync(p, s, productLine, severity, status);
            return Ok(result);
        }

        // GET /api/uw-rules/{ruleId}
        [HttpGet("{ruleId:guid}")]
        public async Task<IActionResult> GetById(Guid ruleId)
        {
            var rule = await _service.GetRuleByIdAsync(ruleId);
            if (rule is null) return NotFound();
            return Ok(rule);
        }

        // GET /api/uw-rules/product-line/{line}
        [HttpGet("product-line/{line}")]
        public async Task<IActionResult> GetByProductLine(string line)
        {
            var rules = await _service.GetRulesByProductLineAsync(line);
            return Ok(rules);
        }

        // GET /api/uw-rules/severity/{severity}
        [HttpGet("severity/{severity}")]
        public async Task<IActionResult> GetBySeverity(Severity severity)
        {
            var rules = await _service.GetRulesBySeverityAsync(severity);
            return Ok(rules);
        }

        // POST /api/uw-rules
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUWRuleDto dto)
        {
            var created = await _service.CreateRuleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { ruleId = created.UWRuleID }, created);
        }

        // PUT /api/uw-rules/{ruleId}
        [HttpPut("{ruleId:guid}")]
        public async Task<IActionResult> Update(Guid ruleId, [FromBody] UpdateUWRuleDto dto)
        {
            var updated = await _service.UpdateRuleAsync(ruleId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // PATCH /api/uw-rules/{ruleId}/status
        [HttpPatch("{ruleId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid ruleId, [FromBody] UpdateRuleStatusDto dto)
        {
            var updated = await _service.UpdateRuleStatusAsync(ruleId, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE /api/uw-rules/{ruleId}
        [HttpDelete("{ruleId:guid}")]
        public async Task<IActionResult> Delete(Guid ruleId)
        {
            var deleted = await _service.DeleteRuleAsync(ruleId);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // POST /api/uw-rules/evaluate/{submissionId}
        [HttpPost("evaluate/{submissionId:guid}")]
        public async Task<IActionResult> Evaluate(Guid submissionId)
        {
            var result = await _service.EvaluateRulesForSubmissionAsync(submissionId);
            return Ok(result);
        }
    }
}
