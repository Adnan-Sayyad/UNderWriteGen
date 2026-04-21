using Microsoft.AspNetCore.Mvc;
using SubmissionAndIntake.Configs.Enums;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.DTOs;

namespace SubmissionAndIntake.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _service;

        public SubmissionsController(ISubmissionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var submissions = await _service.GetAllSubmissionsAsync();
            return Ok(submissions);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var submission = await _service.GetSubmissionByIdAsync(id);
            if (submission is null) return NotFound();
            return Ok(submission);
        }

        [HttpGet("agent/{agentId:guid}")]
        public async Task<IActionResult> GetByAgentId(Guid agentId)
        {
            var submissions = await _service.GetSubmissionsByAgentIdAsync(agentId);
            return Ok(submissions);
        }

        [HttpGet("party/{partyId:guid}")]
        public async Task<IActionResult> GetByPartyId(Guid partyId)
        {
            var submissions = await _service.GetSubmissionsByPartyIdAsync(partyId);
            return Ok(submissions);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(SubmissionStatus status)
        {
            var submissions = await _service.GetSubmissionsByStatusAsync(status);
            return Ok(submissions);
        }

        [HttpGet("productline/{productLine}")]
        public async Task<IActionResult> GetByProductLine(ProductLine productLine)
        {
            var submissions = await _service.GetSubmissionsByProductLineAsync(productLine);
            return Ok(submissions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubmissionDto dto)
        {
            var created = await _service.CreateSubmissionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.SubmissionID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmissionDto dto)
        {
            var updated = await _service.UpdateSubmissionAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteSubmissionAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
