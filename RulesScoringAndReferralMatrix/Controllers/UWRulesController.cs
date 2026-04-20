using Microsoft.AspNetCore.Mvc;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.DTOs;

namespace RulesScoringAndReferralMatrix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UWRulesController : ControllerBase
    {
        private readonly IUWRuleService _service;

        public UWRulesController(IUWRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rules = await _service.GetAllRulesAsync();
            return Ok(rules);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rule = await _service.GetRuleByIdAsync(id);
            if (rule is null) return NotFound();
            return Ok(rule);
        }

        [HttpGet("product/{productLine}")]
        public async Task<IActionResult> GetByProductLine(string productLine)
        {
            var rules = await _service.GetRulesByProductLineAsync(productLine);
            return Ok(rules);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRules()
        {
            var rules = await _service.GetActiveRulesAsync();
            return Ok(rules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUWRuleDto dto)
        {
            var created = await _service.CreateRuleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.UWRuleID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUWRuleDto dto)
        {
            var updated = await _service.UpdateRuleAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteRuleAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
